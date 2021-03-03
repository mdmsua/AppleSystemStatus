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
            log.LogInformation("Importing {country} system status...", response.Country);
            return repositoryService.ImportSystemStatusAsync(response.Country, response.Services);
        }

        [FunctionName(nameof(FetchSystemStatus))]
        public Task<Response> FetchSystemStatus([ActivityTrigger] string country, ILogger log)
        {
            log.LogInformation("Fetching system status for country {country}...", country);
            return systemStatusService.GetSystemStatusAsync(country);
        }

        [FunctionName(nameof(RetrieveCountries))]
        public async Task<IEnumerable<string>> RetrieveCountries([ActivityTrigger] IDurableActivityContext ctx, ILogger log)
        {
            using var scope = log.BeginScope(nameof(RetrieveCountries));
            log.LogInformation("Retrieving countries...");
            var countries = await repositoryService.ExportCountriesAsync();
            return countries.Select(country => country.Id);
        }

        [FunctionName(nameof(FetchCountrySupport))]
        public async Task<KeyValuePair<string, bool>> FetchCountrySupport([ActivityTrigger] string country, ILogger log)
        {
            using var scope = log.BeginScope(nameof(FetchCountrySupport));
            log.LogDebug("Fetching {country} country support...", country);
            bool isSupported = await systemStatusService.IsCountrySupportedAsync(country);
            log.LogDebug("Country {country is {support}", country, isSupported ? "supported" : "not supported");
            return new KeyValuePair<string, bool>(country, isSupported);
        }

        [FunctionName(nameof(ImportCountries))]
        public Task ImportCountries([ActivityTrigger] string[] countries, ILogger log)
        {
            using var scope = log.BeginScope(nameof(ImportCountries));
            log.LogInformation("Importing countries...");
            return repositoryService.ImportCountriesAsync(countries);
        }
    }
}