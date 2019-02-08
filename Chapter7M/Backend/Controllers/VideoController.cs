using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Backend.DataObjects;
using Backend.Models;

namespace Backend.Controllers
{
    public class VideoController : TableController<Video>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Video>(context, Request, enableSoftDelete: true);
        }

        // GET tables/Video
        public IQueryable<Video> GetAllVideo()
        {
            return Query(); 
        }

        // GET tables/Video/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Video> GetVideo(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Video/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Video> PatchVideo(string id, Delta<Video> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Video
        public async Task<IHttpActionResult> PostVideo(Video item)
        {
            Video current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Video/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteVideo(string id)
        {
             return DeleteAsync(id);
        }
    }
}
