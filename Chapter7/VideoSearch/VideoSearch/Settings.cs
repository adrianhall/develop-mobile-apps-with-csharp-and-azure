using System;

namespace VideoSearch
{
    public static class Settings
    {
        public static string AzureSearchUri = "https://zumbook.search.windows.net";

        /// <summary>
        /// Replace this with your API key from the Azure Search.  You should
        /// never check in code with an API key in it - read the key from an
        /// Azure App Service App Setting and then provide it to your mobile
        /// clients via a custom API.
        /// </summary>
        public static string AzureSearchApiKey = "88E95AB69AAAAB6FC5579E1CC40E7FC4";
    }
}
