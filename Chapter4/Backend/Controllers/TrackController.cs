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
    public class TrackController : TableController<Track>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Track>(context, Request);
        }

        // GET tables/Track
        public IQueryable<Track> GetAllTrack()
        {
            return Query();
        }

        // GET tables/Track/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Track> GetTrack(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Track/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Track> PatchTrack(string id, Delta<Track> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/Track
        public async Task<IHttpActionResult> PostTrack(Track item)
        {
            Track current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Track/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTrack(string id)
        {
             return DeleteAsync(id);
        }
    }
}
