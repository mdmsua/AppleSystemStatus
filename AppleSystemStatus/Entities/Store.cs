using System;
using System.Collections.Generic;
using System.Linq;

namespace AppleSystemStatus.Entities
{
    public class Store
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<Service> Services { get; set; } = Enumerable.Empty<Service>().ToList();
    }
}