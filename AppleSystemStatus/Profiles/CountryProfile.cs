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

        private static CultureInfo MapToCultureInfo(string country) => new CultureInfo(country);

        private static RegionInfo MapToRegionInfo(string country) => new RegionInfo(country);

        private static string MapToLanguage(string country) => LanguageRegex.Match(MapToCultureInfo(country).NativeName).Value;

        private static string MapToCountry(string country) => MapToRegionInfo(country).NativeName;

        public CountryProfile()
        {
            CreateMap<CountryEntity, CountryModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z.Id))
                .ForMember(x => x.Name, y => y.MapFrom(z => MapToCountry(z.Id)))
                .ForMember(x => x.Language, y => y.MapFrom(z => MapToLanguage(z.Id)));

            CreateMap<string, CountryModel>()
                .ForMember(x => x.Id, y => y.MapFrom(z => z))
                .ForMember(x => x.Name, y => y.MapFrom(z => MapToCountry(z)))
                .ForMember(x => x.Language, y => y.MapFrom(z => MapToLanguage(z)));
        }
    }
}