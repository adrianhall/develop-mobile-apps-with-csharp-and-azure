using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Chapter3.DataObjects;
using Chapter3.Models;

namespace Chapter3.Controllers
{
    public class TagController : TableController<Tag>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Tag>(context, Request);
        }

        // GET tables/Tag
        public IQueryable<Tag> GetAllTag()
        {
            return Query(); 
        }

        // GET tables/Tag/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Tag> GetTag(string id)
        {
            return Lookup(id);
        }

        [Authorize]
        // PATCH tables/Tag/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Tag> PatchTag(string id, Delta<Tag> patch)
        {
             return UpdateAsync(id, patch);
        }

        [Authorize]
        // POST tables/Tag
        public async Task<IHttpActionResult> PostTag(Tag item)
        {
            Tag current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        [Authorize]
        // DELETE tables/Tag/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTag(string id)
        {
             return DeleteAsync(id);
        }
    }
}
