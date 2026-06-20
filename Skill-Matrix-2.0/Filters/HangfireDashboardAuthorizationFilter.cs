using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Skill_Matrix_2_0.Filters
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // Allow all requests in local development environment
            if (env.IsDevelopment())
            {
                return true;
            }

            var user = httpContext.User;

            // In non-development, if httpContext.User is not authenticated (common for direct browser navigation with JWT headers),
            // attempt to read and parse the token from the query parameter "?token=YOUR_JWT"
            if (user.Identity?.IsAuthenticated != true)
            {
                var token = httpContext.Request.Query["token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);
                        var claims = jwtToken.Claims;
                        var identity = new ClaimsIdentity(claims, "Jwt");
                        user = new ClaimsPrincipal(identity);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // Grant access only to Admin or SuperAdmin users
            return user.Identity?.IsAuthenticated == true &&
                   (user.IsInRole("Admin") || user.IsInRole("SuperAdmin"));
        }
    }
}
