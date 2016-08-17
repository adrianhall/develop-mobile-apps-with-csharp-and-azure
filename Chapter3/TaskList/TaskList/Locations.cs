namespace TaskList
{
    public static class Locations
    {
        public static readonly string AppServiceUrl = "https://book-chapter3.azurewebsites.net";

        public static readonly string AlternateLoginHost = null;

        public static readonly string AadClientId = "9ad80bbd-2f07-442e-841d-b8d6a47927eb";

        public static readonly string AadRedirectUri = $"{AppServiceUrl}/.auth/login/done";

        public static readonly string AadAuthority = "https://login.windows.net/photoadrianoutlook.onmicrosoft.com";
    }
}

