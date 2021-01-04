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
using Microsoft.Data.SqlClient;

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

        public async Task ImportSystemStatusAsync(int store, IEnumerable<ServiceModel> services)
        {
            log.LogDebug("Checking if store {store} exists...", store);
            var useIdentityInsert = false;
            var storeEntity = await context.Stores.Include(x => x.Services).ThenInclude(x => x.Events).SingleOrDefaultAsync(x => x.Id == store);
            if (storeEntity is null)
            {
                log.LogDebug("Store {store} doesn't exist. Creating...", store);
                storeEntity = new Store(store);
                await context.Stores.AddAsync(storeEntity);
                useIdentityInsert = true;
            }
            log.LogDebug("Checking {store} services...", store);
            foreach (var service in services)
            {
                log.LogDebug("Service {service}...", service.ServiceName);
                var serviceEntity = storeEntity.Services.SingleOrDefault(x => x.Name == service.ServiceName);
                if (serviceEntity is null)
                {
                    log.LogDebug("Service {service} doesn't exist. Creating...", service.ServiceName);
                    serviceEntity = new ServiceEntity { Name = service.ServiceName };
                    storeEntity.Services.Add(serviceEntity);
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
                serviceEntity.Status = serviceEntity.Events.SingleOrDefault(e => !e.EpochEndDate.HasValue)?.StatusType;
            }
            if (useIdentityInsert)
            {
                await SaveChangesIdentityInsertAsync();
            }
            else
            {
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Store>> ExportStoresAsync() =>
            await context.Stores.AsNoTracking().ToListAsync();

        public async Task<IEnumerable<ServiceEntity>> ExportServicesAsync(int store) =>
            await context.Services.Where(s => s.StoreId == store).AsNoTracking().ToListAsync();

        public async Task<IEnumerable<EventEntity>> ExportEventsAsync(Guid service) =>
            await context.Events.Where(e => e.ServiceId == service).OrderBy(e => e.EpochEndDate).ThenByDescending(e => EpochStartDate).AsNoTracking().ToListAsync();

        public async Task ImportStoresAsync(IEnumerable<int> stores)
        {
            var storedIds = await context.Stores.Select(s => s.Id).ToListAsync();
            log.LogDebug("Stores in database: {stores}", string.Join(", ", storedIds));
            log.LogDebug("Candidate stores: {stores}", string.Join(", ", stores));
            var absentStores = stores.Except(storedIds).ToList();
            if (absentStores.Count == 0)
            {
                log.LogInformation("No new stores detected");
                return;
            }
            log.LogInformation("Importing {count} new stores: {stores}", absentStores.Count, string.Join(", ", absentStores));
            context.Stores.AddRange(absentStores.Select(x => new Store(x)));
            await SaveChangesIdentityInsertAsync();
        }

        private async Task SaveChangesIdentityInsertAsync()
        {
            await context.Database.OpenConnectionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Stores ON");
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Stores OFF");
            }
            catch (SqlException exception)
            {
                log.LogError(exception, exception.Message);
                throw;
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }
    }
}
