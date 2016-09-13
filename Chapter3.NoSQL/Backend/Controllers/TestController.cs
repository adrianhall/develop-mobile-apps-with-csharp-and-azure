using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Backend.DataObjects;
using Backend.DomainManagers;
using Microsoft.Azure.Mobile.Server;

namespace Backend.Controllers
{
    public class TestController : TableController<Test>
    {
        private const string connectionString = "MS_AzureStorageAccountConnectionString";
        private const string tableName = "Test";

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            DomainManager = new DocumentDbDomainManager<Test>(Request);
        }

        // GET tables/Test
        public IQueryable<Test> GetAllTests()
        {
            return Query();
        }

        // GET tables/Test/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Test> GetTest(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Test/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task<Test> PatchTestAsync(string id, Delta<Test> patch)
        {
            return await UpdateAsync(id, patch);
        }

        // DELETE tables/Test/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public async Task DeleteTestAsync(string id)
        {
            await DeleteAsync(id);
        }

        // POST tables/Test
        public async Task<IHttpActionResult> PostTestAsync(Test item)
        {
            Test current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }
    }
}