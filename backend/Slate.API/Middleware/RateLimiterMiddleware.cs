using Slate.Application.Interfaces;
using StackExchange.Redis;

namespace Slate.API.Middleware;

public class RateLimiterMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
{
    private const int Limit = 100;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    
    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser)
    {
        var db = redis.GetDatabase();
        
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        var clientKey = isAuthenticated
            ? currentUser.UserId.ToString()
            : context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        
        var now = DateTime.UtcNow.Ticks;
        var windowStart = now - (now % Window.Ticks);
        var redisKey = $"rate_limit:{clientKey}:{windowStart}";
        
        var count = await db.StringIncrementAsync(redisKey);
        
        if (count == 1)
            await db.KeyExpireAsync(redisKey, Window);
        
        if (count > Limit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            
            context.Response.Headers.RetryAfter = ((int)Window.TotalSeconds).ToString();
            
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Too many requests. Please try again later." 
            });
            
            return;
        }

        await next(context);
    }
}