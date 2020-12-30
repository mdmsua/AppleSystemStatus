using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AppleSystemStatus.Entities
{
    public class Store
    {
        public Store()
        {

        }

        public Store(int store)
        {
            Id = store;
        }

        public int Id { get; set; }

        [JsonIgnore]
        public ICollection<Service> Services { get; set; } = Enumerable.Empty<Service>().ToList();
    }
}