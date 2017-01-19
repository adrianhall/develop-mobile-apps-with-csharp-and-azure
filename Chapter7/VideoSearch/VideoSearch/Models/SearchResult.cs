using Newtonsoft.Json;

namespace VideoSearch.Models
{
    public class SearchResult
    {
        [JsonProperty(PropertyName = "@search.score")]
        public double SearchScore { get; set; }
    }
}
