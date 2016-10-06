namespace TaskList
{
    public static class Locations
    {
        /// <summary>
        /// The Azure App Service Backend
        /// </summary>
        public static readonly string AppServiceUrl = "https://zumobook-ch4.azurewebsites.net";

        /// <summary>
        /// Set to the App Service Backend if AppServiceUrl is localhost (local dev)
        /// </summary>
        public static readonly string AlternateLoginHost = null;

        /// <summary>
        /// ClientId for AAD Authentication
        /// </summary>
        public static readonly string AadClientId = "e35ff8fb-4ffd-4fcb-aae8-fe795f2745b1";

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
