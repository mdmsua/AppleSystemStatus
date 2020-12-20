using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Newtonsoft.Json;

namespace AppleSystemStatus.Models
{
    public class Event
    {
        [JsonProperty("usersAffected")]
        public string UsersAffected { get; set; } = string.Empty;

        [JsonProperty("epochStartDate")]
        public long EpochStartDate { get; set; }

        [JsonProperty("epochEndDate")]
        public long? EpochEndDate { get; set; }

        [JsonProperty("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonProperty("statusType")]
        public string StatusType { get; set; } = string.Empty;

        [JsonProperty("datePosted")]
        public string DatePosted { get; set; } = string.Empty;

        [JsonProperty("startDate")]
        public string StartDate { get; set; } = string.Empty;

        [JsonProperty("endDate")]
        public string? EndDate { get; set; }

        [JsonProperty("affectedServices")]
        public string[]? AffectedServices { get; set; }

        [JsonProperty("eventStatus")]
        public string EventStatus { get; set; } = string.Empty;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class EventEqualityComparer : IEqualityComparer<Event>
    {
        public bool Equals([AllowNull] Event x, [AllowNull] Event y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if ((x is null && y != default) || (x != default && y is null))
            {
                return false;
            }

            return (x!.AffectedServices ?? Array.Empty<string>()).SequenceEqual(y!.AffectedServices ?? Array.Empty<string>()) &&
                x.DatePosted == y.DatePosted &&
                x.EndDate == y.EndDate &&
                x.EpochEndDate == y.EpochEndDate &&
                x.EpochStartDate == y.EpochStartDate &&
                x.EventStatus == y.EventStatus &&
                x.Message == y.Message &&
                x.MessageId == y.MessageId &&
                x.StartDate == y.StartDate &&
                x.StatusType == y.StatusType &&
                x.UsersAffected == y.UsersAffected;
        }

        public int GetHashCode([DisallowNull] Event obj)
        {
            return obj.AffectedServices.Aggregate(0, (x, y) => x.GetHashCode() ^ y.GetHashCode()) ^
                obj.DatePosted.GetHashCode() ^
                (obj.EndDate ?? string.Empty).GetHashCode() ^
                obj.EpochEndDate.GetHashCode() ^
                obj.EpochStartDate.GetHashCode() ^
                obj.EventStatus.GetHashCode() ^
                obj.Message.GetHashCode() ^
                obj.MessageId.GetHashCode() ^
                obj.StartDate.GetHashCode() ^
                obj.StatusType.GetHashCode() ^
                obj.UsersAffected.GetHashCode();
        }
    }
}