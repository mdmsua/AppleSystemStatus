using Newtonsoft.Json;

namespace AppleSystemStatus.Models
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

        public string Name { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;

        [JsonIgnore]
        public string Region => Id.Replace('-', '_');
    }
}
