using AutoMapper;
using EventModel = AppleSystemStatus.Models.Event;
using EventEntity = AppleSystemStatus.Entities.Event;
using System;
using AppleSystemStatus.Entities;

namespace AppleSystemStatus.Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<EventModel, EventEntity>()
                .ForMember(x => x.AffectedServices, y => y.MapFrom<EventValueResolver>())
                .ForMember(x => x.MessageId, y => y.MapFrom<EventValueResolver>())
                .ForMember(x => x.StatusType, y => y.MapFrom<EventValueResolver>())
                .ForMember(x => x.EventStatus, y => y.MapFrom<EventValueResolver>());
        }

        private class EventValueResolver :
            IValueResolver<EventModel, EventEntity, string?>,
            IValueResolver<EventModel, EventEntity, long>,
            IValueResolver<EventModel, EventEntity, StatusType>,
            IValueResolver<EventModel, EventEntity, EventStatus>
        {
            public string? Resolve(EventModel source, EventEntity destination, string? destMember, ResolutionContext context) =>
                source.AffectedServices is null ? null : string.Join(',', source.AffectedServices);

            public long Resolve(EventModel source, EventEntity destination, long destMember, ResolutionContext context) =>
                long.Parse(source.MessageId);

            public StatusType Resolve(EventModel source, EventEntity destination, StatusType destMember, ResolutionContext context) =>
                source.StatusType switch
                {
                    "Maintenance" => StatusType.Maintenance,
                    "Issue" => StatusType.Issue,
                    "Outage" => StatusType.Outage,
                    _ => throw new NotSupportedException($"{source.StatusType} is not supported")
                };

            public EventStatus Resolve(EventModel source, EventEntity destination, EventStatus destMember, ResolutionContext context) =>
                source.EventStatus switch
                {
                    "completed" => EventStatus.Completed,
                    "ongoing" => EventStatus.Ongoing,
                    "resolved" => EventStatus.Resolved,
                    "upcoming" => EventStatus.Upcoming,
                    _ => throw new NotSupportedException($"{source.EventStatus} is not supported")
                };
        }
    }
}