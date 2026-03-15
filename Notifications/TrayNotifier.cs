using AdzanToolbar.Tray;

namespace AdzanToolbar.Notifications;

internal sealed class TrayNotifier
{
    private readonly TrayHost _trayHost;

    public TrayNotifier(TrayHost trayHost)
    {
        _trayHost = trayHost;
    }

    public void ShowPrayerReminder(string prayerName, string city, string country)
    {
        _trayHost.ShowBalloon(
            $"{prayerName} Adhan",
            $"It is time for {prayerName} in {city}, {country}.");
    }

    public void ShowTest(string city, string country)
    {
        _trayHost.ShowBalloon(
            "Test Notification",
            $"Adhan reminders are active for {city}, {country}.");
    }
}
