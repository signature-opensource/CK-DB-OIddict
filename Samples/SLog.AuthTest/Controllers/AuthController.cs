using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace SLog.AuthTest.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        public AuthController()
        {

        }

        [Route("Logout")]
        [HttpGet]
        public async Task<IActionResult> Logout([FromQuery] string redirectUrl)
        {
            await HttpContext.SignOutAsync();
            return Redirect(redirectUrl);
        }

        [Route("Login")]
        [HttpGet]
        public async Task Login([FromQuery] string redirectUrl)
        {
            await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = redirectUrl
            });
        }
    }
}