using System;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Backend.Controllers
{
    [MobileAppController]
    public class SettingsController : ApiController
    {
        private Controllers.Settings _pSettings;

        public SettingsController()
        {
            _pSettings = new Controllers.Settings
            {
                SearchApiKey = Environment.GetEnvironmentVariable("SEARCH_APIKEY"),
                SearchEndpoint = Environment.GetEnvironmentVariable("SEARCH_ENDPOINT")
            };
        }

        // GET api/Settings
        public Controllers.Settings Get()
        {
            return _pSettings;
        }
    }

    public class Settings
    {
        public string SearchEndpoint { get; set; }

        public string SearchApiKey { get; set; }
    }
}
