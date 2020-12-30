using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AppleSystemStatus.Models;
using AutoMapper;
using Newtonsoft.Json;

namespace AppleSystemStatus.Services
{
    public class SystemStatusService
    {
        private readonly HttpClient client;
        private readonly IMapper mapper;

        private static readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();

        private const string pathFormat = "/support/systemstatus/data/system_status_{0}.js";

        public SystemStatusService(HttpClient client, IMapper mapper)
        {
            this.client = client;
            this.mapper = mapper;
        }

        public async Task<bool> IsStoreSupportedAsync(int store)
        {
            var path = string.Format(pathFormat, mapper.Map<Store>(store).Code);
            using var request = new HttpRequestMessage(HttpMethod.Head, path);
            using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<Response> GetSystemStatusAsync(int store)
        {
            var path = string.Format(pathFormat, mapper.Map<Store>(store).Code);
            using var stream = await client.GetStreamAsync(path);
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var response = jsonSerializer.Deserialize<Response>(jsonReader);
            response.Store = store;
            return response;
        }
    }
}