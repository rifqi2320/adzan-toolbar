using System.Collections.Generic;

namespace AdzanToolbar.Settings;

internal sealed class PrayerPreferences
{
    public bool Fajr { get; set; } = true;

    public bool Dhuhr { get; set; } = true;

    public bool Asr { get; set; } = true;

    public bool Maghrib { get; set; } = true;

    public bool Isha { get; set; } = true;

    public IReadOnlyDictionary<string, bool> AsDictionary() =>
        new Dictionary<string, bool>
        {
            ["Fajr"] = Fajr,
            ["Dhuhr"] = Dhuhr,
            ["Asr"] = Asr,
            ["Maghrib"] = Maghrib,
            ["Isha"] = Isha
        };

    public bool IsEnabled(string prayerName) => prayerName switch
    {
        "Fajr" => Fajr,
        "Dhuhr" => Dhuhr,
        "Asr" => Asr,
        "Maghrib" => Maghrib,
        "Isha" => Isha,
        _ => false
    };
}
