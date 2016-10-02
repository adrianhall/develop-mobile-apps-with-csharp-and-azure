namespace TaskList
{
    public static class Locations
    {
        public static readonly string AppServiceUrl = "https://book-chapter3.azurewebsites.net";

        public static readonly string AlternateLoginHost = null;

        public static readonly string AadClientId = "ea2eb134-80ee-4f5f-b1bd-26d68ce19d08";

        public static readonly string AadRedirectUri = $"{AppServiceUrl}/.auth/login/done";

        public static readonly string AadAuthority = "https://login.windows.net/photoadrianoutlook.onmicrosoft.com";
    }
}

