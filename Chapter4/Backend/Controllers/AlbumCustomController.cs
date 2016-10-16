using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server.Config;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;

namespace Backend.Controllers
{
    [MobileAppController]
    public class AlbumCustomController : ApiController
    {
        MobileServiceContext context;

        public AlbumCustomController() : base()
        {
            context = new MobileServiceContext();
        }

        [HttpPost]
        public async Task<AlbumCustomResponse> PostAsync([FromBody] Album newAlbum)
        {
            // Use a transaction to update the database
            using (DbContextTransaction transaction = context.Database.BeginTransaction())
            {
                try
                {
                    context.Albums.Add(newAlbum);
                    await context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }

            // Now generate whatever output we want.
            AlbumCustomResponse response = new AlbumCustomResponse
            {
                Status = 200
            };
            return response;
        }
    }

    public class AlbumCustomResponse
    {
        public int Status { get; set; }
    }
}
