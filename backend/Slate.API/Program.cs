using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OpenTelemetry.Metrics;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using Slate.API.Handlers;
using Slate.API.Middleware;
using Slate.Application.Common;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;
using Slate.Application.Services;
using Slate.Domain.Repositories;
using Slate.Infrastructure.Persistence;
using Slate.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;
using SecuritySchemeType = Microsoft.OpenApi.SecuritySchemeType;

namespace Slate.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.ConfigureServices(builder.Configuration);

        var app = builder.Build();
        
        // TODO: switch to migrations
        // using (var scope = app.Services.CreateScope())
        // {
        //     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //     await dbContext.Database.EnsureDeletedAsync();
        //     await dbContext.Database.EnsureCreatedAsync();
        // }

        app.ConfigureApp();

        await app.RunAsync();
    }
    
    private static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddSignalR();
        
        // TODO: SignalR Redis Backplane
        // services.AddSignalR().AddStackExchangeRedis(redisConnString, options => {
        //     options.Configuration.ChannelPrefix = "SlateShare";
        // });

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddJsonConsole(options =>
            {
                options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
                options.JsonWriterOptions = new JsonWriterOptions
                {
                    Indented = false
                };
            });
        });
        
        services.AddCors();
        services.AddControllers();
        services.AddServices();
        services.AddRepositories();
        services.AddPostgres(configuration);
        services.AddRedis(configuration);
        services.AddOptions();
        services.AddAuth();
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwagger();
        
        services.AddOpenTelemetryMetrics();
        
        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.AssemblyMarkerTypes = [typeof(Program), typeof(IBoardClient)];
            options.AsyncApi = new AsyncApiDocument
            {
                Info = new Info("Slate AsyncAPI", "1.0.0")
                {
                    Description = "Документация асинхронных событий SignalR"
                }
            };
        });
        
        services.AddRouting(options => options.LowercaseUrls = true);
    }

    private static void AddOpenTelemetryMetrics(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddPrometheusExporter()); 
    }

    private static void AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                
                // policy.AllowAnyOrigin()
                //     .AllowAnyHeader()
                //     .AllowAnyMethod()
                //     .AllowCredentials();
            });
        });
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthCookieService, AuthCookieService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IColumnService, ColumnService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IChecklistService, ChecklistService>();
        services.AddScoped<IUserService, UserService>();
        
        services.AddScoped<IAiService, TornadoAiService>();
        services.AddScoped<IChatMessageService, ChatMessageService>();
        services.AddScoped<TornadoPromptBuilder>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IColumnRepository, ColumnRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IBoardMemberRepository, BoardMemberRepository>();
        
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
    }
    
    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });
    }

    private static void AddOptions(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<AuthCookieOptions>()
            .BindConfiguration(AuthCookieOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOptions<TornadoAiOptions>()
            .BindConfiguration(TornadoAiOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, IOptions<AuthCookieOptions>>((options, jwtOptionsAccessor, authCookieOptionsAccessor) =>
            {
                var jwtOptions = jwtOptionsAccessor.Value;
                var authCookieOptions = authCookieOptionsAccessor.Value;

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (!string.IsNullOrEmpty(context.Token))
                            return Task.CompletedTask;
                        if (context.Request.Cookies.TryGetValue(authCookieOptions.Name, out var token))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
    }
    
    private static void AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static void AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(connectionString));
    }

    private static void ConfigureApp(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
        
        app.UseExceptionHandler();
        
        app.UseCors("AllowFrontend");
        
        app.UseHttpsRedirection();
        
        app.UseForwardedHeaders();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseMiddleware<RateLimiterMiddleware>();
        
        app.MapControllers();

        app.MapHub<BoardHub>("/hubs/board");

        app.MapAsyncApiDocuments();
        app.MapAsyncApiUi();
        
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}