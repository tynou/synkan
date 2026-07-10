using Hangfire.Dashboard;

namespace Synkan.API;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true; 
    }
}