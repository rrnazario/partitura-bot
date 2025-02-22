using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TelegramPartHook.Application.Requests;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Infrastructure.Attributes;

namespace TelegramPartHook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController
        : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost]        
        public async Task<IActionResult> VerifyUserAsync(LoginRequest loginRequest)
        {
            return Ok(await _loginService.VerifyUserAsync(loginRequest));
        }

        [PortalAuthorize]
        [HttpPost("logout")]        
        public async Task<IActionResult> LogoutAsync(LogoutRequest logoutRequest)
        {
            return Ok(await _loginService.LogoutAsync(logoutRequest));
        }
    }
}
