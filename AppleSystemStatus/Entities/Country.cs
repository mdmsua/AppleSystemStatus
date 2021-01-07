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

        public Country(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        [JsonIgnore]
        public ICollection<Service> Services { get; set; } = Enumerable.Empty<Service>().ToList();
    }
}