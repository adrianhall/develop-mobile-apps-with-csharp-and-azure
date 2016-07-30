using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using Backend.CustomAuth.Models;
using Microsoft.Azure.Mobile.Server.Login;

namespace Backend.CustomAuth.Controllers
{
    [Route(".auth/login/custom")]
    public class CustomAuthController : ApiController
    {
        private MobileServiceContext db;
        private string signingKey, audience, issuer;

        public CustomAuthController()
        {
            System.Diagnostics.Debug.WriteLine("IN CUSTOMAUTHCONTROLLER");
            db = new MobileServiceContext();
            signingKey = Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY");
            System.Diagnostics.Debug.WriteLine($"CUSTOMAUTHCONTROLLER: signingKey = {signingKey}");
            var website = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            System.Diagnostics.Debug.WriteLine($"CUSTOMAUTHCONTROLLER: website = {website}");
            audience = $"https://{website}";
            issuer = $"https://{website}";
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] User body)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsValidUser(body))
            {
                return Unauthorized();
            }

            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, body.Username)
            };

            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(
                claims, signingKey, audience, issuer, TimeSpan.FromDays(30));
            return Ok(new LoginResult()
            {
                AuthenticationToken = token.RawData,
                User = new LoginResultUser { UserId = body.Username }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool IsValidUser(User user)
        {
            return db.Users.Count(u => u.Username.Equals(user.Username) && u.Password.Equals(user.Password)) > 0;
        }
    }

    public class LoginResult
    {
        public string AuthenticationToken { get; set; }
        public LoginResultUser User { get; set; }
    }

    public class LoginResultUser
    {
        public string UserId { get; set; }
    }
}