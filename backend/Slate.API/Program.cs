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
        
    }

    private static void ConfigureApp(this WebApplication app)
    {
        
    }
    
}