using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AdzanToolbar.Settings;

internal static class LocationCatalog
{
    private static readonly IReadOnlyDictionary<string, string[]> SuggestedCitiesByCountry =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Indonesia"] =
            [
                "Ambon", "Balikpapan", "Banda Aceh", "Bandar Lampung", "Bandung", "Banjarmasin",
                "Batam", "Bekasi", "Bogor", "Cirebon", "Denpasar", "Depok", "Gorontalo", "Jakarta",
                "Jambi", "Jayapura", "Kediri", "Kupang", "Madiun", "Makassar", "Malang", "Manado",
                "Mataram", "Medan", "Padang", "Palangkaraya", "Palembang", "Palu", "Pekalongan",
                "Pekanbaru", "Pontianak", "Samarinda", "Semarang", "Serang", "Solo", "Sorong",
                "Surabaya", "Tangerang", "Tarakan", "Tasikmalaya", "Ternate", "Yogyakarta"
            ],
            ["Malaysia"] = ["Johor Bahru", "Kota Bharu", "Kota Kinabalu", "Kuala Lumpur", "Kuching", "Malacca", "Penang", "Putrajaya"],
            ["Singapore"] = ["Singapore"],
            ["Brunei"] = ["Bandar Seri Begawan"],
            ["Saudi Arabia"] = ["Jeddah", "Madinah", "Makkah", "Riyadh"],
            ["United Arab Emirates"] = ["Abu Dhabi", "Dubai", "Sharjah"],
            ["United Kingdom"] = ["Birmingham", "Glasgow", "Leeds", "Leicester", "London", "Manchester"],
            ["United States"] = ["Chicago", "Houston", "Jersey City, NJ", "Los Angeles", "New York", "Washington, DC"],
            ["Australia"] = ["Adelaide", "Brisbane", "Canberra", "Melbourne", "Perth", "Sydney"],
            ["Turkey"] = ["Ankara", "Bursa", "Istanbul", "Izmir", "Konya"],
            ["Pakistan"] = ["Islamabad", "Karachi", "Lahore"],
            ["India"] = ["Bengaluru", "Chennai", "Hyderabad", "Kolkata", "Mumbai", "New Delhi"],
            ["Egypt"] = ["Alexandria", "Cairo", "Giza"],
            ["Morocco"] = ["Casablanca", "Marrakesh", "Rabat", "Tangier"]
        };

    public static IReadOnlyList<string> GetCountries()
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name).EnglishName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToArray();
    }

    public static IReadOnlyList<string> GetCities(string country, IReadOnlyList<SavedLocation> recentLocations)
    {
        var suggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (SuggestedCitiesByCountry.TryGetValue(country, out var suggestedCities))
        {
            foreach (var city in suggestedCities)
            {
                suggestions.Add(city);
            }
        }

        foreach (var recent in recentLocations.Where(location => string.Equals(location.Country, country, StringComparison.OrdinalIgnoreCase)))
        {
            suggestions.Add(recent.City);
        }

        return suggestions.OrderBy(city => city).ToArray();
    }
}
