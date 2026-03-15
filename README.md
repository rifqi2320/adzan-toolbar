# Adzan Toolbar

Lightweight Windows tray app for adhan reminders. It runs as a WinForms tray process, fetches prayer times from the AlAdhan API, and shows a custom popup window for selected prayers.

## Features

- Runs in the Windows system tray with no main window
- Fetches cached weekly prayer times from `https://api.aladhan.com/v1/calendarByCity`
- Lets the user configure city, country, enabled prayers, and reminder lead time
- Persists settings in `%AppData%/AdzanToolbar/config.json`
- Stores prayer cache data for up to 7 days in `%AppData%/AdzanToolbar/prayer-cache.json`
- Uses a custom borderless popup window instead of tray balloon tips for reminders
- Supports smaller framework-dependent single-file publishing with `dotnet publish`

## Structure

- `App/AdzanApplicationContext.cs`: application lifetime and event wiring
- `Tray/TrayHost.cs`: `NotifyIcon` and tray menu
- `Scheduling/AdhanScheduler.cs`: periodic polling and duplicate-prevention
- `Services/AlAdhanClient.cs`: AlAdhan API integration
- `Services/PrayerScheduleRepository.cs`: weekly fetch + cache-backed schedule lookups
- `Notifications/PopupNotifier.cs`: custom popup reminders
- `Settings/`: JSON settings storage, location suggestions, and settings dialog

## Local Run

Install the .NET 8 SDK on Windows, then run:

```bash
dotnet run
```

## Publish

Smaller single-file Windows build that uses the installed .NET Desktop Runtime:

```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true
```

## Notes

- The calculation method is fixed to `20` (KEMENAG Indonesia).
- The scheduler polls every 30 seconds.
- The app uses the AlAdhan response timezone when the local .NET runtime can resolve it. If Windows cannot resolve the timezone ID, it falls back to the machine's local offset.
