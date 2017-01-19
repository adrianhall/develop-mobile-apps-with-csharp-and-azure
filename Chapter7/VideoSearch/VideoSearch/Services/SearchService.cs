using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VideoSearch.Models;

namespace VideoSearch.Services
{
    public class SearchService
    {
        private HttpClient _client;
        private string _apiVersion = "2016-09-01";

        public SearchService()
        {
            this._client = new HttpClient();
        }

        public async Task<List<Movie>> SearchMoviesAsync(string searchTerms)
        {
            var content = await SearchAsync("videos", searchTerms);
            var movieResults = JsonConvert.DeserializeObject<MovieResults>(content);
            return movieResults.Movies;
        }

        private async Task<string> SearchAsync(string index, string searchTerms)
        {
            var uri = new UriBuilder($"{Settings.AzureSearchUri}/indexes/${index}/docs");
            uri.Query = $"api-version={_apiVersion}&search={Uri.EscapeDataString(searchTerms)}";

            var request = new HttpRequestMessage
            {
                RequestUri = uri.Uri,
                Method = HttpMethod.Get
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("api-key", Settings.AzureSearchApiKey);

            var response = await _client.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
