namespace TaskList
{
    public static class Locations
    {
        /// <summary>
        /// The Azure App Service Backend
        /// </summary>
        public static readonly string AppServiceUrl = "https://7372523aa2ec4d2cbd1186ce29e5fd92.azurewebsites.net";

        /// <summary>
        /// Set to the App Service Backend if AppServiceUrl is localhost (local dev)
        /// </summary>
        public static readonly string AlternateLoginHost = null;

        /// <summary>
        /// ClientId for AAD Authentication
        /// </summary>
        public static readonly string AadClientId = "7dcc8b8e-45ca-4c6c-8bc1-5c4a78730218";

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
