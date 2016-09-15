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
    }
}
