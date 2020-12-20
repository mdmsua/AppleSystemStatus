using System;
using System.Collections.Generic;
using System.Linq;

namespace AppleSystemStatus.Entities
{
    public class Service
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid StoreId { get; set; }
#nullable disable
        public Store Store { get; set; }
#nullable enable

        public ICollection<Event> Events { get; set; } = Array.Empty<Event>().ToList();
    }
}