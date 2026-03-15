namespace AdzanToolbar.Settings;

internal sealed class AppSettings
{
    public string City { get; set; } = "Jakarta";

    public string Country { get; set; } = "Indonesia";

    public int CalculationMethod { get; set; } = 20;

    public int ReminderLeadMinutes { get; set; } = 0;

    public int PollingIntervalSeconds { get; set; } = 30;

    public PrayerPreferences Prayers { get; set; } = new();
}
