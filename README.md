# Adzan Toolbar

Lightweight Windows tray app for adhan reminders. It runs as a WinForms tray process, fetches prayer times from the AlAdhan API, and shows tray balloon notifications for selected prayers.

## Features

- Runs in the Windows system tray with no main window
- Fetches daily prayer times from `https://api.aladhan.com/v1/timingsByCity`
- Lets the user configure city, country, calculation method, enabled prayers, and reminder lead time
- Persists settings in `%AppData%/AdzanToolbar/config.json`
- Supports single-file publishing with `dotnet publish`

## Structure

- `App/AdzanApplicationContext.cs`: application lifetime and event wiring
- `Tray/TrayHost.cs`: `NotifyIcon` and tray menu
- `Scheduling/AdhanScheduler.cs`: periodic polling and duplicate-prevention
- `Services/AlAdhanClient.cs`: AlAdhan API integration
- `Notifications/TrayNotifier.cs`: balloon tip notifications
- `Settings/`: JSON settings storage and settings dialog

## Local Run

Install the .NET 8 SDK on Windows, then run:

```bash
dotnet run
```

## Publish

Single-file Windows build:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Notes

- Default configuration is `Jakarta, Indonesia` with calculation method `20` (KEMENAG).
- The app uses the AlAdhan response timezone when the local .NET runtime can resolve it. If Windows cannot resolve the timezone ID, it falls back to the machine's local offset.
