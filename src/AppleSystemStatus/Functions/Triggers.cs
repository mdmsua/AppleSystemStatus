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
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppleSystemStatus.Functions
{
    public class Triggers
    {
        private readonly RepositoryService repository;
        private readonly IMapper mapper;
        private readonly HealthCheckService healthCheck;

        public Triggers(RepositoryService repository, IMapper mapper, HealthCheckService healthCheck)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.healthCheck = healthCheck;
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

        [FunctionName(nameof(CountriesImportHttp))]
        public static async Task<IActionResult> CountriesImportHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "countries")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await StartCountriesImportOrchestrationAsync(starter, log);
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

        [FunctionName(nameof(CountriesImportCron))]
        public static async Task CountriesImportCron(
            [TimerTrigger("0 0 0 * * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            await StartCountriesImportOrchestrationAsync(starter, log);
        }

        [FunctionName(nameof(Purge))]
        public static async Task Purge(
            [TimerTrigger("59 59 23 28 * *")] TimerInfo timer,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            await starter.PurgeInstanceHistoryAsync(DateTime.MinValue, DateTime.UtcNow, Enum.GetValues(typeof(OrchestrationStatus)).Cast<OrchestrationStatus>());
        }

        [FunctionName(nameof(CountriesExportHttp))]
        public async Task<IActionResult> CountriesExportHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "countries")] HttpRequest req,
            ILogger log)
        {
            var countries = await repository.ExportCountriesAsync();
            return new OkObjectResult(countries.Select(country => mapper.Map<Country>(country)).OrderBy(country => country.Name));
        }

        [FunctionName(nameof(ServicesExportHttp))]
        public async Task<IActionResult> ServicesExportHttp(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "countries/{country}/services")] HttpRequest req,
            string country,
            ILogger log)
        {
            var services = await repository.ExportServicesAsync(country);
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

        [FunctionName(nameof(HealthCheck))]
        public async Task<IActionResult> HealthCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "healthcheck")] HttpRequest req,
            ILogger log)
        {
            var healthReport = await healthCheck.CheckHealthAsync();
            if (healthReport.Status == HealthStatus.Healthy)
            {
                return new OkResult();
            }

            var exception = new AggregateException(healthReport.Entries.Select(e => e.Value.Exception));
            log.LogCritical(exception, "App is unhealthy: {0}", string.Join(Environment.NewLine, healthReport.Entries.Select(e => $"{e.Key}: {e.Value.Description}")));
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }

        [FunctionName(nameof(Warmup))]
        public async Task Warmup([WarmupTrigger] WarmupContext warmup) =>
            await healthCheck.CheckHealthAsync();

        private static async Task<string> StartSystemStatusOrchestrationAsync(IDurableOrchestrationClient orchestrator, ILogger log)
        {
            string instanceId = await orchestrator.StartNewAsync(nameof(Orchestrators.SystemStatus), null);
            log.LogInformation($"Started system status orchestration with ID = '{instanceId}'.");
            return instanceId;
        }

        private static async Task<string> StartCountriesImportOrchestrationAsync(IDurableOrchestrationClient orchestrator, ILogger log)
        {
            string instanceId = await orchestrator.StartNewAsync(nameof(Orchestrators.CountriesImport), null);
            log.LogInformation($"Started counties import orchestration with ID = '{instanceId}'.");
            return instanceId;
        }
    }
}
