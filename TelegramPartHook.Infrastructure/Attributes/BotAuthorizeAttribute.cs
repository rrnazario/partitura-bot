using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using TelegramPartHook.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace TelegramPartHook.Infrastructure.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BotAuthorizeAttribute : TypeFilterAttribute
    {
        public BotAuthorizeAttribute()
            : base(typeof(BotAuthorizeFilter)) { }
    }

    public class BotAuthorizeFilter : IAuthorizationFilter
    {
        public BotAuthorizeFilter() { }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string authHeader = context.HttpContext.Request.Headers["Authorization"];
                if (authHeader != null)
                {
                    var authHeaderValue = AuthenticationHeaderValue.Parse(authHeader);
                    if (authHeaderValue.Scheme.Equals(AuthenticationSchemes.Basic.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        var credentials = Encoding.UTF8
                                            .GetString(Convert.FromBase64String(authHeaderValue.Parameter ?? string.Empty))
                                            .Split(':', 2);
                        if (credentials.Length == 2)
                        {
                            if (IsAuthorized(context, credentials[0], credentials[1]))
                            {
                                return;
                            }
                        }
                    }
                }

                ReturnUnauthorizedResult(context);
            }
            catch (FormatException)
            {
                ReturnUnauthorizedResult(context);
            }
        }

        private void ReturnUnauthorizedResult(AuthorizationFilterContext context)
        {
            // Return 401 and a basic authentication challenge (causes browser to show login dialog)
            context.HttpContext.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"\"";

            throw new UnauthorizedAccessException();
        }

        private bool IsAuthorized(AuthorizationFilterContext context, string username, string password)
            {
                var adminConfiguration = context.HttpContext.RequestServices.GetRequiredService<IAdminConfiguration>();
                               
                return adminConfiguration.Users.TryGetValue(username, out var pass) && password.Equals(pass);
            }
    }
}
