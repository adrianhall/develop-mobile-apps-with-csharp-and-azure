using Backend.Models;
using Microsoft.Azure.Mobile.Server;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;

namespace Backend.Controllers
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

        // PATCH tables/Tag/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Tag> PatchTag(string id, Delta<Tag> patch)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        // POST tables/Tag
        public IHttpActionResult PostTag(Tag item)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        // DELETE tables/Tag/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTag(string id)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }
    }
}
