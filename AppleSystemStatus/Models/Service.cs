using System;
using Newtonsoft.Json;

namespace AppleSystemStatus.Models
{
    public class Service
    {
        [JsonProperty("redirectUrl")]
        public Uri? RedirectUrl { get; set; }

        [JsonProperty("events")]
        public Event[] Events { get; set; } = Array.Empty<Event>();

        [JsonProperty("serviceName")]
        public string ServiceName { get; set; } = string.Empty;
    }
}