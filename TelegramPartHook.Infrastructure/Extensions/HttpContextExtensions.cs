using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace TelegramPartHook.Application.Extensions
{
    public static class HttpContextExtensions
    {
        public static string ExtractPortalUser(this HttpContext context)
        {
            var token = ExtractSecurityToken(context);

            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;

            return tokenS!.Audiences.First();
        }

        public static string ExtractSecurityToken(this HttpContext context)
            => context.Request.Headers["Authorization"].FirstOrDefault()!.Split(" ").LastOrDefault()!;
    }
}
