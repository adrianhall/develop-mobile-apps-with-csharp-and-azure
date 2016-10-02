using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Backend.DataObjects;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;

namespace Backend.Controllers
{
    [MobileAppController]
    public class TodoItemStorageController : StorageController<TodoItem>
    {
        [HttpPost]
        [Route("tables/TodoItem/{id}/StorageToken")]
        public async Task<HttpResponseMessage> StorageTokenAsync(string id, StorageTokenRequest value)
            => Request.CreateResponse(await GetStorageTokenAsync(id, value));

        [HttpGet]
        [Route("tables/TodoItem/{id}/MobileServiceFiles")]
        public async Task<HttpResponseMessage> GetFilesAsync(string id)
            => Request.CreateResponse(await GetRecordFilesAsync(id));

        [HttpDelete]
        [Route("tables/TodoItem/{id}/MobileServiceFiles/{name}")]
        public Task DeleteAsync(string id, string name) => base.DeleteFileAsync(id, name);
    }
}
