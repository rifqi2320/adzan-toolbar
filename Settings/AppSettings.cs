namespace AdzanToolbar.Settings;

internal sealed class AppSettings
{
    public string City { get; set; } = "Jakarta";

    public string Country { get; set; } = "Indonesia";

    public int ReminderLeadMinutes { get; set; } = 0;

    public PrayerPreferences Prayers { get; set; } = new();

    public List<SavedLocation> RecentLocations { get; set; } = [];
}
