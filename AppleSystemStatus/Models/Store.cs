using Newtonsoft.Json;

namespace AppleSystemStatus.Models
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
        public string Code { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;
    }
}
