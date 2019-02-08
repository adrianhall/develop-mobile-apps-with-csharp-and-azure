using System.Collections.Generic;
using Newtonsoft.Json;

namespace VideoSearch.Models
{
    public class MovieResults
    {
        [JsonProperty(PropertyName = "@odata.context")]
        public string Context { get; set; }

        [JsonProperty(PropertyName = "value")]
        public List<Movie> Movies { get; set; }
    }
}
