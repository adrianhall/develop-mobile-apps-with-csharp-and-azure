using Backend.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Files;
using Microsoft.Azure.Mobile.Server.Files.Controllers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Backend.Controllers
{
    [MobileAppController]
    public class TagStorageController : StorageController<Tag>
    {
        [HttpPost]
        [Route("tables/TodoItem/{id}/StorageToken")]
        public HttpResponseMessage StorageToken(string id, StorageTokenRequest value)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }

        [HttpGet]
        [Route("tables/TodoItem/{id}/MobileServiceFiles")]
        public HttpResponseMessage GetFiles(string id)
            => Request.CreateResponse(new List<MobileServiceFile>());

        [HttpDelete]
        [Route("tables/TodoItem/{id}/MobileServiceFiles/{name}")]
        public Task DeleteAsync(string id, string name)
        {
            throw new HttpResponseException(HttpStatusCode.Forbidden);
        }
    }
}
