using AutoMapper;
using StoreModel = AppleSystemStatus.Models.Store;
using StoreEntity = AppleSystemStatus.Entities.Store;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace AppleSystemStatus.Profiles
{
    public class StoreProfile : Profile
    {
        private static readonly Regex LanguageRegex = new Regex("\\w+", RegexOptions.Compiled);

        private static string MapToStoreCode(int store) => MapToCultureInfo(store).Name.Replace("-", "_");

        private static CultureInfo MapToCultureInfo(int store)
        {
            try
            {
                return new CultureInfo(store);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static RegionInfo MapToRegionInfo(int store) => new RegionInfo(store);

        private static string MapToLanguage(int store) => LanguageRegex.Match(MapToCultureInfo(store).NativeName).Value;

        private static string MapToCountry(int store) => MapToRegionInfo(store).NativeName;

        public StoreProfile()
        {
            CreateMap<StoreEntity, StoreModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z.Id))
                .ForMember(x => x.Code, y => y.MapFrom(z => MapToStoreCode(z.Id)))
                .ForMember(x => x.Country, y => y.MapFrom(z => MapToCountry(z.Id)))
                .ForMember(x => x.Language, y => y.MapFrom(z => MapToLanguage(z.Id)));

            CreateMap<int, StoreModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z))
                .ForMember(x => x.Code, y => y.MapFrom(z => MapToStoreCode(z)))
                .ForMember(x => x.Country, y => y.MapFrom(z => MapToCountry(z)))
                .ForMember(x => x.Language, y => y.MapFrom(z => MapToLanguage(z)));
        }
    }
}