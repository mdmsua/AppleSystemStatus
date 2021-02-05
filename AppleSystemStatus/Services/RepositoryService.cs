using System.Collections.Generic;
using System.Threading.Tasks;

using ServiceModel = AppleSystemStatus.Models.Service;
using ServiceEntity = AppleSystemStatus.Entities.Service;
using EventModel = AppleSystemStatus.Models.Event;
using EventEntity = AppleSystemStatus.Entities.Event;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AppleSystemStatus.Entities;
using System.Linq;
using AutoMapper;
using System;

namespace AppleSystemStatus.Services
{
    public class RepositoryService
    {
        private readonly AppleSystemStatusDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<RepositoryService> log;

        public RepositoryService(AppleSystemStatusDbContext context, IMapper mapper, ILogger<RepositoryService> log)
        {
            this.context = context;
            this.mapper = mapper;
            this.log = log;
        }

        public async Task ImportSystemStatusAsync(string country, IEnumerable<ServiceModel> services)
        {
            log.LogDebug("Checking if country {country} exists...", country);
            var countryEntity = await context.Countries.Include(x => x.Services).ThenInclude(x => x.Events).SingleOrDefaultAsync(x => x.Id == country);
            if (countryEntity is null)
            {
                log.LogDebug("Country {country} doesn't exist. Creating...", country);
                countryEntity = new Country(country);
                await context.Countries.AddAsync(countryEntity);
            }
            log.LogDebug("Checking {country} services...", country);
            foreach (var service in services)
            {
                log.LogDebug("Service {service}...", service.ServiceName);
                var serviceEntity = countryEntity.Services.SingleOrDefault(x => x.Name == service.ServiceName);
                if (serviceEntity is null)
                {
                    log.LogDebug("Service {service} doesn't exist. Creating...", service.ServiceName);
                    serviceEntity = new ServiceEntity { Name = service.ServiceName };
                    countryEntity.Services.Add(serviceEntity);
                }
                log.LogDebug("Checking {service} events...", service.ServiceName);
                foreach (var @event in service.Events)
                {
                    log.LogDebug("Event {EpochStartDate}...", @event.EpochStartDate);
                    var eventEntity = serviceEntity.Events.SingleOrDefault(x => x.EpochStartDate == @event.EpochStartDate);
                    if (eventEntity is null)
                    {
                        log.LogDebug("Event {EpochStartDate} doesn't exist. Creating...", @event.EpochStartDate);
                        eventEntity = mapper.Map<EventModel, EventEntity>(@event);
                        serviceEntity.Events.Add(eventEntity);
                    }
                    else
                    {
                        log.LogDebug("Event {EpochStartDate} already exist. Checking epoch end date...", @event.EpochStartDate);
                        if (@event.EpochEndDate.HasValue && !eventEntity.EpochEndDate.HasValue)
                        {
                            log.LogDebug("Event {EpochStartDate} has epoch end date {EpochEndDate}. Updating...", @event.EpochStartDate, @event.EpochEndDate);
                            eventEntity = mapper.Map<EventModel, EventEntity>(@event);
                        }
                        else
                        {
                            log.LogDebug("Event {EpochStartDate} still has no epoch end date. Skipping.", @event.EpochStartDate);
                        }
                    }
                }
                serviceEntity.Status = serviceEntity.Events.FirstOrDefault(e => !e.EpochEndDate.HasValue)?.StatusType;
            }
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Country>> ExportCountriesAsync() =>
            await context.Countries.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<ServiceEntity>> ExportServicesAsync(string country) =>
            await context.Services.Where(s => s.CountryId == country).AsNoTracking().ToListAsync();

        public async Task<IEnumerable<EventEntity>> ExportEventsAsync(Guid service) =>
            await context.Events.Where(e => e.ServiceId == service).OrderBy(e => e.EpochEndDate).ThenByDescending(e => e.EpochStartDate).AsNoTracking().ToListAsync();

        public async Task ImportCountriesAsync(IEnumerable<string> countries)
        {
            var presentCountries = await context.Countries.Select(s => s.Id).ToListAsync();
            log.LogDebug("Countries in database: {countries}", string.Join(", ", presentCountries));
            log.LogDebug("Candidate countries: {countries}", string.Join(", ", countries));
            var absentCountries = countries.Except(presentCountries).ToList();
            if (absentCountries.Count == 0)
            {
                log.LogInformation("No new countries detected");
                return;
            }
            log.LogInformation("Importing {count} new countries: {countries}", absentCountries.Count, string.Join(", ", absentCountries));
            context.Countries.AddRange(absentCountries.Select(x => new Country(x)));
            await context.SaveChangesAsync();
        }
    }
}
