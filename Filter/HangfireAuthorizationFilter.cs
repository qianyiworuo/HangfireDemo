using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace HangfireDemo.Filter
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            // 生产环境应添加真实认证逻辑
            //var httpContext = context.GetHttpContext();
            //return httpContext.User.Identity?.IsAuthenticated ?? false;

            // 开发环境直接允许访问：
             return true; 
        }
    }
}
