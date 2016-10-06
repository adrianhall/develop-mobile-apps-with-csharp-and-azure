using Backend.DataObjects;
using Backend.Models;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Backend.Controllers
{
    [Authorize]
    [MobileAppController]
    public class TodoItemStorageController : StorageController<TodoItem>
    {
        EntityDomainManager<TodoItem> domainManager;

        public TodoItemStorageController()
        {
            MobileServiceContext context = new MobileServiceContext();
            domainManager = new EntityDomainManager<TodoItem>(context, Request, enableSoftDelete: true);
        }

        [HttpPost]
        [Route("tables/TodoItem/{id}/StorageToken")]
        public async Task<HttpResponseMessage> StorageTokenAsync(string id, StorageTokenRequest value)
        {
            if (!await IsRecordOwner(id))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return Request.CreateResponse(await GetStorageTokenAsync(id, value));
        }

        [HttpGet]
        [Route("tables/TodoItem/{id}/MobileServiceFiles")]
        public async Task<HttpResponseMessage> GetFilesAsync(string id)
        {
            if (!await IsRecordOwner(id))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            return Request.CreateResponse(await GetRecordFilesAsync(id));
        }

        [HttpDelete]
        [Route("tables/TodoItem/{id}/MobileServiceFiles/{name}")]
        public async Task DeleteAsync(string id, string name)
        {
            if (!await IsRecordOwner(id))
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
            await base.DeleteFileAsync(id, name);
        }

        /// <summary>
        /// Check to see if the current user is the owner of the record
        /// </summary>
        /// <param name="id">The id of the record to check</param>
        /// <returns>True if the user is the record owner</returns>
        private async Task<bool> IsRecordOwner(string id)
        {
            var principal = this.User as ClaimsPrincipal;
            var sid = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            var item = (await domainManager.LookupAsync(id)).Queryable.FirstOrDefault();
            return item.UserId.Equals(sid);
        }
    }
}
