using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AppleSystemStatus.Models;
using Newtonsoft.Json;

namespace AppleSystemStatus.Services
{
    public class SystemStatusService
    {
        private readonly HttpClient client;

        private static readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();

        private const string pathFormat = "/support/systemstatus/data/system_status_{0}.js";

        public SystemStatusService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<bool> IsStoreSupportedAsync(string store)
        {
            var path = string.Format(pathFormat, store);
            using var request = new HttpRequestMessage(HttpMethod.Head, path);
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<Response> GetSystemStatusAsync(string store)
        {
            var path = string.Format(pathFormat, store);
            using var stream = await client.GetStreamAsync(path);
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var response = jsonSerializer.Deserialize<Response>(jsonReader);
            response.Store = store;
            return response;
        }
    }
}