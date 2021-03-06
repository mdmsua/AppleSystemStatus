using System;
using Newtonsoft.Json;

namespace AppleSystemStatus.Models
{
    public class Response
    {
        [JsonProperty("services")]
        public Service[] Services { get; set; } = Array.Empty<Service>();

        public string Country { get; set; } = string.Empty;
    }
}