using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backend.Helpers
{
    public static class Webhook
    {
        public static async Task<HttpStatusCode> SendAsync<T>(Uri uri, T data)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = uri;
            var response = await httpClient.PostAsJsonAsync<T>("", data);
            return response.StatusCode;
        }
    }
}
