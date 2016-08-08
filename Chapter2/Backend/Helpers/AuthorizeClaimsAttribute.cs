using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Azure.Mobile.Server.Authentication;

namespace Backend.Helpers
{
    public class AuthorizeClaimsAttribute : AuthorizationFilterAttribute
    {
        string Type { get; }
        string Value { get; }

        public AuthorizeClaimsAttribute(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var request = actionContext.Request;
            var user = actionContext.RequestContext.Principal;
            if (user != null)
            {
                var identity = await user.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(request);
                var countOfMatchingClaims = identity.UserClaims
                    .Where(c => c.Type.Equals(Type) && c.Value.Equals(Value))
                    .Count();
                if (countOfMatchingClaims > 0) return;

            }
            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }
    }
}