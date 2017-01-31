using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VideoSearch.Models
{
    public class Movie : SearchResult
    {
        [JsonProperty(PropertyName = "videoId")]
        public string Id { get; set; }

        public string Title { get; set; }

        public Uri Image { get; set; }

        public double Rating { get; set; }

        public int ReleaseYear { get; set; }

        [JsonProperty(PropertyName = "genre")]
        public List<string> Genres { get; set; }
    }
}
