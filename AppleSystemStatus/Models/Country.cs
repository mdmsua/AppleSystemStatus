using Newtonsoft.Json;

namespace AppleSystemStatus.Models
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
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Language { get; set; } = string.Empty;
    }
}
