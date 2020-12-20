using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AppleSystemStatus.Models;
using AppleSystemStatus.Services;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AppleSystemStatus.Functions
{
    public class Activities
    {
        private readonly RepositoryService repositoryService;
        private readonly SystemStatusService systemStatusService;

        public Activities(RepositoryService repositoryService, SystemStatusService systemStatusService)
        {
            this.repositoryService = repositoryService;
            this.systemStatusService = systemStatusService;
        }

        private static readonly EventEqualityComparer eventEqualityComparer = new EventEqualityComparer();

        [FunctionName(nameof(ImportSystemStatus))]
        public Task ImportSystemStatus([ActivityTrigger] Response response, ILogger log)
        {
            using var scope = log.BeginScope(nameof(ImportSystemStatus));
            log.LogInformation("Importing {store} system status...", response.Store);
            return repositoryService.ImportSystemStatusAsync(response.Store, response.Services);
        }

        [FunctionName(nameof(FetchSystemStatus))]
        public Task<Response> FetchSystemStatus([ActivityTrigger] string store, ILogger log)
        {
            log.LogInformation("Fetching system status for store {store}...", store);
            return systemStatusService.GetSystemStatusAsync(store);
        }

        [FunctionName(nameof(RetrieveStores))]
        public Task<string[]> RetrieveStores([ActivityTrigger] IDurableActivityContext ctx, ILogger log)
        {
            using var scope = log.BeginScope(nameof(RetrieveStores));
            log.LogInformation("Retrieving stores...");
            return repositoryService.ExportStoresAsync().ContinueWith(r => r.Result.ToArray());
        }

        [FunctionName(nameof(FetchStoreSupport))]
        public async Task<KeyValuePair<string, bool>> FetchStoreSupport([ActivityTrigger] string store, ILogger log)
        {
            using var scope = log.BeginScope(nameof(FetchStoreSupport));
            log.LogDebug("Fetching {store} store support...", store);
            bool isSupported = await systemStatusService.IsStoreSupportedAsync(store);
            log.LogDebug("Store {store} is {support}", store, isSupported ? "supported" : "not supported");
            return new KeyValuePair<string, bool>(store, isSupported);
        }

        [FunctionName(nameof(ImportStores))]
        public Task ImportStores([ActivityTrigger] string[] stores, ILogger log)
        {
            using var scope = log.BeginScope(nameof(ImportStores));
            log.LogInformation("Importing stores...");
            return repositoryService.ImportStoresAsync(stores);
        }
    }
}