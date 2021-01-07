using AutoMapper;
using CountryModel = AppleSystemStatus.Models.Country;
using CountryEntity = AppleSystemStatus.Entities.Country;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AppleSystemStatus.Profiles
{
    public class CountryProfile : Profile
    {
        private static readonly Regex LanguageRegex = new Regex("\\w+", RegexOptions.Compiled);

        private static string MapToCountryCode(int country) => MapToCultureInfo(country).Name.Replace("-", "_");

        private static CultureInfo MapToCultureInfo(int country) => new CultureInfo(country);

        private static RegionInfo MapToRegionInfo(int country) => new RegionInfo(country);

        private static string MapToLanguage(int country) => LanguageRegex.Match(MapToCultureInfo(country).NativeName).Value;

        private static string MapToCountry(int country) => MapToRegionInfo(country).NativeName;

        public CountryProfile()
        {
            CreateMap<CountryEntity, CountryModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z.Id))
                .ForMember(x => x.Code, y => y.MapFrom(z => MapToCountryCode(z.Id)))
                .ForMember(x => x.Name, y => y.MapFrom(z => MapToCountry(z.Id)))
                .ForMember(x => x.Language, y => y.MapFrom(z => MapToLanguage(z.Id)));

            CreateMap<int, CountryModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z))
                .ForMember(x => x.Code, y => y.MapFrom(z => MapToCountryCode(z)))
                .ForMember(x => x.Name, y => y.MapFrom(z => MapToCountry(z)))
                .ForMember(x => x.Language, y => y.MapFrom(z => MapToLanguage(z)));
        }
    }
}