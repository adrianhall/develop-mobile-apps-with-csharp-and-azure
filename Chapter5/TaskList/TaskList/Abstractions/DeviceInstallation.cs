using System.Collections.Generic;
using Newtonsoft.Json;

namespace TaskList.Abstractions
{
    public class DeviceInstallation
    {
        public DeviceInstallation()
        {
            ExpirationTime = "";
            Tags = new List<string>();
            Templates = new Dictionary<string, DeviceTemplate>();
        }

        [JsonProperty(PropertyName = "installationId")]
        public string InstallationId { get; set; }

        [JsonProperty(PropertyName = "expirationTime")]
        public string ExpirationTime { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "pushChannel")]
        public string PushChannel { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "templates")]
        public Dictionary<string, DeviceTemplate> Templates { get; set; }
    }

    public class DeviceTemplate
    {
        public DeviceTemplate()
        {
            Body = "";
            Tags = new List<string>();
        }

        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> Headers { get; set; }

    }
}
