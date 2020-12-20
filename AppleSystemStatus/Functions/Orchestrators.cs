using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using AppleSystemStatus.Models;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AppleSystemStatus.Functions
{
    public static class Orchestrators
    {
        [FunctionName(nameof(SystemStatus))]
        public static async Task SystemStatus(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            if (!context.IsReplaying)
            {
                log.LogInformation("Retrieving stores...");
            }

            var stores = await context.CallActivityAsync<string[]>(nameof(Activities.RetrieveStores), default);

            if (!context.IsReplaying)
            {
                log.LogInformation("Retrieved {count} stores: {stores}", stores.Length, string.Join(", ", stores));
                log.LogInformation("Fetching system status for stores...");
            }

            var fetchTasks = stores.Select(region => context.CallActivityAsync<Response>(nameof(Activities.FetchSystemStatus), region)).ToArray();

            var fetchResults = await Task.WhenAll(fetchTasks);

            if (!context.IsReplaying)
            {
                log.LogInformation("System status fetching done. Importing...");
            }

            var importTasks = fetchResults.Select(result => context.CallActivityAsync(nameof(Activities.ImportSystemStatus), result));

            await Task.WhenAll(importTasks);

            if (!context.IsReplaying)
            {
                log.LogInformation("System status import done");
            }
        }

        [FunctionName(nameof(StoresImport))]
        public static async Task StoresImport(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(culture => culture.Name.Replace('-', '_'));
            var tasks = cultures.Select(culture => context.CallActivityWithRetryAsync<KeyValuePair<string, bool>>(nameof(Activities.FetchStoreSupport), new RetryOptions(TimeSpan.FromSeconds(1), 1), culture));
            var results = await Task.WhenAll(tasks);
            var stores = results.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            if (!context.IsReplaying)
            {
                log.LogInformation("Fetched {count} supported stores: {stores}", stores.Count, string.Join(", ", stores));
            }
            await context.CallActivityAsync(nameof(Activities.ImportStores), stores);
        }
    }
}