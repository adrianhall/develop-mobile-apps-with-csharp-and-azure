using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Chapter3.DataObjects;
using Chapter3.Extensions;
using Chapter3.Models;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;

namespace Chapter3.Controllers
{
    [Authorize]
    public class ExampleController : TableController<Example>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Example>(context, Request, enableSoftDelete: true);
        }

        /// <summary>
        /// Get the list of groups from the claims 
        /// </summary>
        /// <returns>The list of groups</returns>
        public async Task<List<string>> GetGroups()
        {
            var creds = await User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(Request);
            return creds.UserClaims
                .Where(claim => claim.Type.Equals("groups"))
                .Select(claim => claim.Value)
                .ToList();
        }

        /// <summary>
        /// Validator to determine if the provided group is in the list of groups
        /// </summary>
        /// <param name="group">The group name</param>
        public async Task ValidateGroup(string group)
        {
            var groups = await GetGroups();
            if (!groups.Contains(group))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }

        // GET tables/Example
        public async Task<IQueryable<Example>> GetAllExample()
        {
            var groups = await GetGroups();
            return Query().PerGroupFilter(groups); 
        }

        // GET tables/Example/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<SingleResult<Example>> GetExample(string id)
        {
            var groups = await GetGroups();
            return new SingleResult<Example>(Lookup(id).Queryable.PerGroupFilter(groups));
        }

        // PATCH tables/Example/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<Example> PatchExample(string id, Delta<Example> patch)
        {
            await ValidateGroup(patch.GetEntity().GroupId);
            return await UpdateAsync(id, patch);
        }

        // POST tables/Example
        public async Task<IHttpActionResult> PostExample(Example item)
        {
            await ValidateGroup(item.GroupId);
            Example current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Example/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteExample(string id)
        {
            return DeleteAsync(id);
        }
    }
}
