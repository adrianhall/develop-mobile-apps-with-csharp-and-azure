using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Backend.Controllers
{
    [MobileAppController]
    public class ValuesController : ApiController
    {
        // GET api/Default
        public string Get()
        {
            return "Hello from custom controller!";
        }
    }
}
