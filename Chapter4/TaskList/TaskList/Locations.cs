namespace TaskList
{
    public static class Locations
    {
        /// <summary>
        /// The Azure App Service Backend
        /// </summary>
        public static readonly string AppServiceUrl = "https://.azurewebsites.net";

        /// <summary>
        /// Set to the App Service Backend if AppServiceUrl is localhost (local dev)
        /// </summary>
        public static readonly string AlternateLoginHost = null;

        /// <summary>
        /// ClientId for AAD Authentication
        /// </summary>
        public static readonly string AadClientId = "b61c7d68-2086-43a1-a8c9-d93c5732cc84";

        /// <summary>
        /// The AAD Service Authority
        /// </summary>
        public static readonly string AadAuthority = "https://login.windows.net/photoadrianoutlook.onmicrosoft.com";

        /// <summary>
        /// The Redirect URI - always made up from the AppServiceUrl unless there is a good reason not to
        /// </summary>
        public static readonly string AadRedirectUri = $"{AppServiceUrl}/.auth/login/done";
    }
}
