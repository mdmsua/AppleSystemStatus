using System;
using Newtonsoft.Json;

namespace AppleSystemStatus.Models
{
    public class Response
    {
        [JsonProperty("services")]
        public Service[] Services { get; set; } = Array.Empty<Service>();

        public int Country { get; set; }
    }
}