using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using TelegramPartHook.Application.Extensions;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PortalAuthorizeAttribute : TypeFilterAttribute
    {
        public PortalAuthorizeAttribute() 
            : base(typeof(PortalAuthorizeFilter)) { }
    }

    public class PortalAuthorizeFilter(IUserRepository repository) 
        : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var portalUser = context.HttpContext.ExtractPortalUser();

            var user = repository.GetByVipNameAsync(portalUser).GetAwaiter().GetResult();

            if (user is null || !user.IsTokenValid(context.HttpContext.ExtractSecurityToken()))
            {
                ReturnUnauthorizedResult(context);
            }
        }

        private static void ReturnUnauthorizedResult(AuthorizationFilterContext context)
        {
            // Return 401 and a basic authentication challenge (causes browser to show login dialog)
            context.HttpContext.Response.Headers["Authorization"] = $"bearer \"\"";
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            throw new UnauthorizedAccessException();
        }
    }
}
