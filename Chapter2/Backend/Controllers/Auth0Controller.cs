using System;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;
using Backend.CustomAuth.Models;
using Microsoft.Azure.Mobile.Server.Login;

namespace Backend.CustomAuth.Controllers
{
    [Route(".auth/login/auth0")]
    public class Auth0Controller : ApiController
    {
        private JwtSecurityTokenHandler tokenHandler;
        private string clientID, domain;
        private string signingKey, audience, issuer;

        public Auth0Controller()
        {
            // Information for the incoming Auth0 Token
            domain = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
            clientID = Environment.GetEnvironmentVariable("AUTH0_CLIENTID");

            // Information for the outgoing ZUMO Token
            signingKey = Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY");
            var website = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            audience = $"https://{website}/";
            issuer = $"https://{website}/";

            // Token Handler
            tokenHandler = new JwtSecurityTokenHandler();
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody] Auth0User body)
        {
            if (body == null || body.access_token == null || body.access_token.Length == 0)
            {
                return BadRequest();
            }

            try
            {
                var token = (JwtSecurityToken)tokenHandler.ReadToken(body.access_token);
                if (!IsValidUser(token))
                {
                    return Unauthorized();
                }

                var subject = token.Claims.FirstOrDefault(c => c.Type.Equals("sub"))?.Value;
                var email = token.Claims.FirstOrDefault(c => c.Type.Equals("email"))?.Value;
                if (subject == null || email == null)
                {
                    return BadRequest();
                }

                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, subject),
                    new Claim(JwtRegisteredClaimNames.Email, email)
                };

                JwtSecurityToken zumoToken = AppServiceLoginHandler.CreateToken(
                    claims, signingKey, audience, issuer, TimeSpan.FromDays(30));
                return Ok(new LoginResult()
                {
                    AuthenticationToken = zumoToken.RawData,
                    User = new LoginResultUser { UserId = email }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Auth0 JWT Exception = {ex.Message}");
                throw ex;
            }
        }

        private bool IsValidUser(JwtSecurityToken token)
        {
            if (token == null)
                return false;
            var audience = token.Audiences.FirstOrDefault();
            if (!audience.Equals(clientID))
                return false;
            if (!token.Issuer.Equals($"https://{domain}/"))
                return false;
            if (token.ValidTo.AddMinutes(5) < DateTime.Now)
                return false;
            return true;
        }
    }

    public class Auth0User
    {
        public string access_token { get; set; }
    }
}
