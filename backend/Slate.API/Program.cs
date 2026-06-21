using Microsoft.EntityFrameworkCore;
using Slate.Infrastructure.Persistence;

namespace Slate.API;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.ConfigureServices(builder.Configuration);

        var app = builder.Build();

        app.ConfigureApp();

        await app.RunAsync();
    }
    
    private static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgres(configuration);
    }
    
    private static void AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
    }

    private static void ConfigureApp(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}