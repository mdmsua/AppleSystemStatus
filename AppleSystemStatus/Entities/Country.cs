using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace AppleSystemStatus.Entities
{
    public class Country
    {
        public Country()
        {

        }

        public Country(string id)
        {
            Id = id;
        }

        public string Id { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Service> Services { get; set; } = Enumerable.Empty<Service>().ToList();
    }
}