using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using AppleSystemStatus.Models;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AppleSystemStatus.Functions
{
    public static class Orchestrators
    {
        private static readonly Regex NumberPostfix = new Regex("\\d+$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex CountryPostfix = new Regex("-[A-Z]{2}$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex RegionRegex = new Regex("-\\w+-", RegexOptions.Compiled | RegexOptions.Singleline);

        [FunctionName(nameof(SystemStatus))]
        public static async Task SystemStatus(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            if (!context.IsReplaying)
            {
                log.LogInformation("Retrieving countries...");
            }

            var countries = await context.CallActivityAsync<string[]>(nameof(Activities.RetrieveCountries), default);

            if (!context.IsReplaying)
            {
                log.LogInformation("Retrieved {count} counties: {counties}", countries.Length, string.Join(", ", countries));
                log.LogInformation("Fetching system status for counties...");
            }

            var fetchTasks = countries.Select(region => context.CallActivityAsync<Response>(nameof(Activities.FetchSystemStatus), region)).ToArray();

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

        [FunctionName(nameof(CountriesImport))]
        public static async Task CountriesImport(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var cultures = new HashSet<string>(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(TransformCulture).Where(FilterCulture));
            var tasks = cultures.Select(culture => context.CallActivityWithRetryAsync<KeyValuePair<string, bool>>(nameof(Activities.FetchCountrySupport), new RetryOptions(TimeSpan.FromSeconds(1), 1), culture));
            var results = await Task.WhenAll(tasks);
            var countries = results.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            if (!context.IsReplaying)
            {
                log.LogInformation("Fetched {count} supported counties: {counties}", countries.Count, string.Join(", ", countries));
            }
            await context.CallActivityAsync(nameof(Activities.ImportCountries), countries);
        }

        private static string TransformCulture(CultureInfo culture) =>
            RegionRegex.Replace(culture.Name, "-");

        private static bool FilterCulture(string culture) =>
            !NumberPostfix.IsMatch(culture) && CountryPostfix.IsMatch(culture);
    }
}