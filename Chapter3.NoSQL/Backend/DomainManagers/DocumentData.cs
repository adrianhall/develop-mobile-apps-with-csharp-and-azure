using System;
using Microsoft.Azure.Mobile.Server.Tables;
using Newtonsoft.Json;

namespace Backend.DomainManagers
{
    public class DocumentData : ITableData
    {
        [JsonProperty(PropertyName="createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty(PropertyName="deleted")]
        public bool Deleted { get; set; }

        [JsonProperty(PropertyName="id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName="updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty(PropertyName="version")]
        public byte[] Version { get; set; }
    }
}