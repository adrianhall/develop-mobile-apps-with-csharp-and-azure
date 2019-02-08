using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Chapter3.DataObjects;
using Chapter3.Models;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Login;
using Newtonsoft.Json;

namespace Chapter3.Controllers
{
    [Authorize]
    [Route("auth/login/custom")]
    public class CustomAuthController : ApiController
    {
        MobileServiceContext dbContext;

        public CustomAuthController()
        {
            dbContext = new MobileServiceContext();

            string website = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            Audience = $"https://{website}/";
            Issuer = $"https://{website}/";
            SigningKey = Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY");
        }

        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string SigningKey { get; set; }

        [HttpGet]
        public async Task<IHttpActionResult> Get()
        {
            var creds = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
            var sid = ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier).Value;

            string name, email;
            try
            {
                email = creds.UserId;
                name = creds.UserClaims.FirstOrDefault(claim => claim.Type.Equals("name")).Value;
            }
            catch (Exception)
            {
                return BadRequest("Email or Name is not present");
            }

            try
            {
                // Insert the record information into the database
                User user = new User()
                {
                    Id = sid,
                    Name = name,
                    EmailAddress = email
                };
                dbContext.Users.AddOrUpdate(user);
                dbContext.SaveChanges();
            } catch (DbUpdateException ex)
            {
                return InternalServerError(ex);
            }

            // Mind a new token based on the old one plus the new information
            var newClaims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, sid),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("name", name)
            };
            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(
                newClaims, SigningKey, Audience, Issuer, TimeSpan.FromDays(30));

            // Return the token and user ID to the client
            return Ok(new LoginResult()
            {
                AuthenticationToken = token.RawData,
                UserId = sid
            });
        }
    }

    public class LoginResult
    {
        [JsonProperty(PropertyName = "authenticationToken")]
        public string AuthenticationToken { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }
    }
}