using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppleSystemStatus.Entities
{
    public class Service
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public Guid StoreId { get; set; }
#nullable disable
        [JsonIgnore]
        public Store Store { get; set; }
#nullable enable

        [JsonIgnore]
        public ICollection<Event> Events { get; set; } = Array.Empty<Event>().ToList();

        [JsonConverter(typeof(StringEnumConverter))]
        public StatusType? Status { get; set; }
    }
}