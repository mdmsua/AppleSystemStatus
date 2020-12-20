using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AppleSystemStatus.Functions
{
    public static class Triggers
    {
        [FunctionName(nameof(SystemStatusHttpTrigger))]
        public static async Task<IActionResult> SystemStatusHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "systemstatus")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await StartSystemStatusOrchestrationAsync(starter, log);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(nameof(StoresImportHttpTrigger))]
        public static async Task<IActionResult> StoresImportHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await StartStoresImportrchestrationAsync(starter, log);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // [FunctionName(nameof(SystemStatusCronTrigger))]
        // public static async Task SystemStatusCronTrigger(
        //     [TimerTrigger("0 * * * * *")]TimerInfo timer,
        //     [DurableClient] IDurableOrchestrationClient starter,
        //     ILogger log)
        // {
        //     await StartSystemStatusOrchestrationAsync(starter, log);
        // }

        // [FunctionName(nameof(StoresImportCronTrigger))]
        // public static async Task StoresImportCronTrigger(
        //     [TimerTrigger("0 0 0 * * *")]TimerInfo timer,
        //     [DurableClient] IDurableOrchestrationClient starter,
        //     ILogger log)
        // {
        //     await StartStoresImportrchestrationAsync(starter, log);
        // }

        private static async Task<string> StartSystemStatusOrchestrationAsync(IDurableOrchestrationClient orchestrator, ILogger log)
        {
            string instanceId = await orchestrator.StartNewAsync(nameof(Orchestrators.SystemStatus), null);
            log.LogInformation($"Started system status orchestration with ID = '{instanceId}'.");
            return instanceId;
        }

        private static async Task<string> StartStoresImportrchestrationAsync(IDurableOrchestrationClient orchestrator, ILogger log)
        {
            string instanceId = await orchestrator.StartNewAsync(nameof(Orchestrators.StoresImport), null);
            log.LogInformation($"Started stores import orchestration with ID = '{instanceId}'.");
            return instanceId;
        }
    }
}
