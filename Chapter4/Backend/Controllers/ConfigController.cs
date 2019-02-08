using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using System.Collections.Generic;
using System;

namespace Backend.Controllers
{
    [MobileAppController]
    public class ConfigController : ApiController
    {
        private ConfigViewModel configuration;

        public ConfigController()
        {
            Dictionary<string, ProviderInformation> providers = new Dictionary<string, ProviderInformation>();

            AddToProviders(providers, "aad", "WEBSITE_AUTH_CLIENT_ID");
            AddToProviders(providers, "facebook", "WEBSITE_AUTH_FB_APP_ID");
            AddToProviders(providers, "google", "WEBSITE_AUTH_GOOGLE_CLIENT_ID");
            AddToProviders(providers, "microsoftaccount", "WEBSITE_AUTH_MSA_CLIENT_ID");
            AddToProviders(providers, "twitter", "WEBSITE_AUTH_TWITTER_CONSUMER_KEY");

            configuration = new ConfigViewModel
            {
                AuthProviders = providers
            };
        }

        private void AddToProviders(Dictionary<string, ProviderInformation> providers, string provider, string envVar)
        {
            string envVal = Environment.GetEnvironmentVariable(envVar);
            if (envVal != null && envVal?.Length > 0)
            {
                providers.Add(provider, new ProviderInformation { ClientId = envVal });
            }

        }

        [HttpGet]
        public ConfigViewModel Get()
        {
            return configuration;
        }
    }

    public class ProviderInformation
    {
        public string ClientId { get; set; }
    }

    public class ConfigViewModel
    {
        public Dictionary<string, ProviderInformation> AuthProviders { get; set; }
    }
}
