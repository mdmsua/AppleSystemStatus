using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AppleSystemStatus.Entities
{
    public class Event
    {
        [JsonIgnore]
        public Guid ServiceId { get; set; }

        public string UsersAffected { get; set; } = string.Empty;

        public long EpochStartDate { get; set; }

        public long? EpochEndDate { get; set; }

        [JsonIgnore]
        public long MessageId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StatusType StatusType { get; set; }

        public string DatePosted { get; set; } = string.Empty;

        public string StartDate { get; set; } = string.Empty;

        public string? EndDate { get; set; }

        public string? AffectedServices { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EventStatus EventStatus { get; set; }

        public string Message { get; set; } = string.Empty;

#nullable disable
        [JsonIgnore]
        public Service Service { get; set; }
#nullable enable
    }
}