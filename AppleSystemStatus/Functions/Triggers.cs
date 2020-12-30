using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using AppleSystemStatus.Services;
using System;
using AutoMapper;
using AppleSystemStatus.Models;
using System.Linq;
using DurableTask.Core;

namespace AppleSystemStatus.Functions
{
    public class Triggers
    {
        private readonly RepositoryService repository;
        private readonly IMapper mapper;

        public Triggers(RepositoryService repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [FunctionName(nameof(SystemStatusHttp))]
        public static async Task<IActionResult> SystemStatusHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "systemstatus")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await StartSystemStatusOrchestrationAsync(starter, log);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(nameof(StoresImportHttp))]
        public static async Task<IActionResult> StoresImportHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stores")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await StartStoresImportrchestrationAsync(starter, log);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(nameof(SystemStatusCron))]
        public static async Task SystemStatusCron(
            [TimerTrigger("0 * * * * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            await StartSystemStatusOrchestrationAsync(starter, log);
        }

        [FunctionName(nameof(StoresImportCron))]
        public static async Task StoresImportCron(
            [TimerTrigger("0 0 0 * * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            await StartStoresImportrchestrationAsync(starter, log);
        }

        [FunctionName(nameof(Purge))]
        public static async Task Purge(
            [TimerTrigger("59 59 23 28 * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            await starter.PurgeInstanceHistoryAsync(DateTime.MinValue, DateTime.UtcNow, Enum.GetValues(typeof(OrchestrationStatus)).Cast<OrchestrationStatus>());
        }

        [FunctionName(nameof(StoresExportHttp))]
        public async Task<IActionResult> StoresExportHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "stores")] HttpRequest req,
            ILogger log)
        {
            var stores = await repository.ExportStoresAsync();
            return new OkObjectResult(stores.Select(store => mapper.Map<Store>(store)).OrderBy(store => store.Country));
        }

        [FunctionName(nameof(ServicesExportHttp))]
        public async Task<IActionResult> ServicesExportHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "stores/{store}/services")] HttpRequest req,
            int store,
            ILogger log)
        {
            var services = await repository.ExportServicesAsync(store);
            return new OkObjectResult(services);
        }

        [FunctionName(nameof(EventsExportHttp))]
        public async Task<IActionResult> EventsExportHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "services/{service}/events")] HttpRequest req,
            Guid service,
            ILogger log)
        {
            var events = await repository.ExportEventsAsync(service);
            return new OkObjectResult(events);
        }

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
