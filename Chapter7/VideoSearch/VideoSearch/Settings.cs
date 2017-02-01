using System;

namespace VideoSearch
{
    public static class Settings
    {
        public static string AzureSearchUri = "https://zumobook.search.windows.net";

        /// <summary>
        /// Replace this with your API key from the Azure Search.  You should
        /// never check in code with an API key in it - read the key from an
        /// Azure App Service App Setting and then provide it to your mobile
        /// clients via a custom API.  This API key will not work!
        /// </summary>
        public static string AzureSearchApiKey = "FDABD27A15A9862FAF4D982CA074DD3E";
    }
}
