namespace TaskList.Helpers
{
    public static class Locations
    {
        public static readonly string AppServiceUrl = "https://zumobook.azurewebsites.net";
        public static readonly string AlternateLoginHost = null;

        public static readonly string AadClientId = "75e20edb-2809-4849-8fd8-89d26ed41016";
        public static readonly string AadRedirectUri = $"{AppServiceUrl}/.auth/login/done";
        public static readonly string AadAuthority = "https://login.windows.net/photoadrianoutlook.onmicrosoft.com";
        public static readonly string CommonAuthority = "https://login.windows.net/common";
    }
}
