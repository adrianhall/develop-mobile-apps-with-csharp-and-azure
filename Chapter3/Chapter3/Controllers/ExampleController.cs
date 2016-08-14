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
    public class ExampleController : TableController<Example>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Example>(context, Request, enableSoftDelete: true);
        }

        // GET tables/Example
        public IQueryable<Example> GetAllExample()
        {
            return Query(); 
        }

        // GET tables/Example/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Example> GetExample(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Example/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Example> PatchExample(string id, Delta<Example> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Example
        public async Task<IHttpActionResult> PostExample(Example item)
        {
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
