using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AdzanToolbar.Models;
using AdzanToolbar.Services;
using AdzanToolbar.Theme;
using AdzanToolbar.Tray;

namespace AdzanToolbar.Settings;

internal sealed class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly PrayerScheduleRepository _scheduleRepository;
    private readonly Icon _windowIcon;
    private readonly ComboBox _countryComboBox;
    private readonly ComboBox _cityComboBox;
    private readonly NumericUpDown _leadMinutesNumeric;
    private readonly CheckBox _fajrCheckBox;
    private readonly CheckBox _dhuhrCheckBox;
    private readonly CheckBox _asrCheckBox;
    private readonly CheckBox _maghribCheckBox;
    private readonly CheckBox _ishaCheckBox;
    private readonly DataGridView _weekGrid;
    private readonly Label _statusLabel;
    private readonly Button _refreshButton;

    public SettingsForm(AppSettings settings, PrayerScheduleRepository scheduleRepository)
    {
        _settings = CloneSettings(settings);
        _scheduleRepository = scheduleRepository;

        Text = "Adzan Toolbar Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(860, 690);
        BackColor = IslamicTheme.Parchment;
        Font = IslamicTheme.BodyFont(10f);
        _windowIcon = TrayIconFactory.Create();
        Icon = _windowIcon;
        FormClosed += (_, _) => _windowIcon.Dispose();

        var root = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = IslamicTheme.Parchment
        };

        var header = new IslamicHeaderPanel
        {
            TitleText = "Adzan Companion",
            SubtitleText = string.Empty
        };

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 16, 18, 18),
            BackColor = IslamicTheme.Parchment
        };

        var controlsTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 4,
            BackColor = IslamicTheme.Parchment
        };
        controlsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        controlsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        controlsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        controlsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        _countryComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems,
            Dock = DockStyle.Fill
        };
        _countryComboBox.Items.AddRange(LocationCatalog.GetCountries().Cast<object>().ToArray());
        _countryComboBox.Text = _settings.Country;
        _countryComboBox.SelectedIndexChanged += (_, _) => RefreshCitySuggestions();
        _countryComboBox.TextChanged += (_, _) => RefreshCitySuggestions();

        _cityComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.CustomSource,
            Dock = DockStyle.Fill
        };
        _cityComboBox.Text = _settings.City;

        _leadMinutesNumeric = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 120,
            Value = _settings.ReminderLeadMinutes,
            Dock = DockStyle.Left,
            Width = 100
        };

        IslamicTheme.StyleInput(_countryComboBox);
        IslamicTheme.StyleInput(_cityComboBox);
        IslamicTheme.StyleInput(_leadMinutesNumeric);

        _fajrCheckBox = CreatePrayerToggle("Fajr", _settings.Prayers.Fajr);
        _dhuhrCheckBox = CreatePrayerToggle("Dhuhr", _settings.Prayers.Dhuhr);
        _asrCheckBox = CreatePrayerToggle("Asr", _settings.Prayers.Asr);
        _maghribCheckBox = CreatePrayerToggle("Maghrib", _settings.Prayers.Maghrib);
        _ishaCheckBox = CreatePrayerToggle("Isha", _settings.Prayers.Isha);

        var prayersPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 4, 0, 12)
        };
        prayersPanel.Controls.AddRange(
            [_fajrCheckBox, _dhuhrCheckBox, _asrCheckBox, _maghribCheckBox, _ishaCheckBox]);

        _refreshButton = new Button
        {
            Text = "Load Week"
        };
        IslamicTheme.StyleButton(_refreshButton, primary: false);
        _refreshButton.Click += async (_, _) => await LoadWeekAsync().ConfigureAwait(true);

        controlsTable.Controls.Add(CreateLabel("Country"), 0, 0);
        controlsTable.Controls.Add(_countryComboBox, 1, 0);
        controlsTable.Controls.Add(CreateLabel("City"), 2, 0);
        controlsTable.Controls.Add(_cityComboBox, 3, 0);
        controlsTable.Controls.Add(CreateLabel("Lead Minutes"), 0, 1);
        controlsTable.Controls.Add(_leadMinutesNumeric, 1, 1);
        controlsTable.Controls.Add(CreateLabel("Prayers"), 0, 2);
        controlsTable.Controls.Add(prayersPanel, 1, 2);
        controlsTable.SetColumnSpan(prayersPanel, 3);
        controlsTable.Controls.Add(_refreshButton, 3, 3);

        _statusLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            Margin = new Padding(0, 8, 0, 8),
            ForeColor = IslamicTheme.Rosewood,
            Font = IslamicTheme.BodyFont(9.5f)
        };

        _weekGrid = CreateWeekGrid();
        var gridCard = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(250, 246, 236),
            BorderStyle = BorderStyle.FixedSingle
        };
        gridCard.Controls.Add(_weekGrid);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 54,
            FlowDirection = FlowDirection.RightToLeft
        };

        var saveButton = new Button
        {
            Text = "Save"
        };
        IslamicTheme.StyleButton(saveButton, primary: true);
        saveButton.Click += (_, _) => SaveAndClose();

        var cancelButton = new Button
        {
            Text = "Cancel"
        };
        IslamicTheme.StyleButton(cancelButton, primary: false);
        cancelButton.Click += (_, _) => Close();

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        contentPanel.Controls.Add(gridCard);
        contentPanel.Controls.Add(_statusLabel);
        contentPanel.Controls.Add(controlsTable);
        contentPanel.Controls.Add(buttonPanel);

        root.Controls.Add(contentPanel);
        root.Controls.Add(header);
        Controls.Add(root);

        Shown += async (_, _) =>
        {
            RefreshCitySuggestions();
            await LoadWeekAsync().ConfigureAwait(true);
        };
    }

    public event EventHandler<AppSettings>? SettingsSaved;

    private async Task LoadWeekAsync()
    {
        var city = _cityComboBox.Text.Trim();
        var country = _countryComboBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
        {
            _statusLabel.Text = "City and country are required.";
            return;
        }

        try
        {
            ToggleLoading(true, "Loading weekly prayer times...");
            var week = await _scheduleRepository.GetPrayerWeekAsync(city, country, default).ConfigureAwait(true);
            BindWeekTable(week);
            _statusLabel.Text = $"{city}, {country}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = ex.Message;
            _weekGrid.Rows.Clear();
        }
        finally
        {
            ToggleLoading(false, _statusLabel.Text);
        }
    }

    private void SaveAndClose()
    {
        var city = _cityComboBox.Text.Trim();
        var country = _countryComboBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(country))
        {
            MessageBox.Show(
                "City and country are required.",
                "Validation Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var settings = new AppSettings
        {
            City = city,
            Country = country,
            ReminderLeadMinutes = Decimal.ToInt32(_leadMinutesNumeric.Value),
            Prayers = new PrayerPreferences
            {
                Fajr = _fajrCheckBox.Checked,
                Dhuhr = _dhuhrCheckBox.Checked,
                Asr = _asrCheckBox.Checked,
                Maghrib = _maghribCheckBox.Checked,
                Isha = _ishaCheckBox.Checked
            },
            RecentLocations = BuildRecentLocations(city, country)
        };

        SettingsSaved?.Invoke(this, settings);
        Close();
    }

    private void RefreshCitySuggestions()
    {
        var country = _countryComboBox.Text.Trim();
        var cities = LocationCatalog.GetCities(country, _settings.RecentLocations);

        _cityComboBox.BeginUpdate();
        _cityComboBox.Items.Clear();
        _cityComboBox.Items.AddRange(cities.Cast<object>().ToArray());
        _cityComboBox.EndUpdate();

        var autoComplete = new AutoCompleteStringCollection();
        autoComplete.AddRange(cities.ToArray());
        _cityComboBox.AutoCompleteCustomSource = autoComplete;
    }

    private void BindWeekTable(IReadOnlyList<PrayerDaySchedule> week)
    {
        _weekGrid.Rows.Clear();
        foreach (var day in week)
        {
            _weekGrid.Rows.Add(
                day.Date.ToString("ddd, dd MMM"),
                day.Fajr,
                day.Dhuhr,
                day.Asr,
                day.Maghrib,
                day.Isha);
        }
    }

    private void ToggleLoading(bool isLoading, string statusText)
    {
        _refreshButton.Enabled = !isLoading;
        _weekGrid.Enabled = !isLoading;
        _countryComboBox.Enabled = !isLoading;
        _cityComboBox.Enabled = !isLoading;
        _statusLabel.Text = statusText;
    }

    private List<SavedLocation> BuildRecentLocations(string city, string country)
    {
        return _settings.RecentLocations
            .Prepend(new SavedLocation { City = city, Country = country })
            .DistinctBy(location => $"{location.City}|{location.Country}")
            .Take(20)
            .ToList();
    }

    private static DataGridView CreateWeekGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            GridColor = IslamicTheme.ParchmentDark,
            EnableHeadersVisualStyles = false,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = IslamicTheme.Emerald700,
                ForeColor = IslamicTheme.Parchment,
                Font = IslamicTheme.BodyFont(10f, FontStyle.Bold)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = IslamicTheme.Ink,
                SelectionBackColor = Color.FromArgb(230, IslamicTheme.Gold300),
                SelectionForeColor = IslamicTheme.Ink,
                Font = IslamicTheme.BodyFont(10f)
            }
        };

        grid.Columns.Add("Date", "Date");
        grid.Columns.Add("Fajr", "Fajr");
        grid.Columns.Add("Dhuhr", "Dhuhr");
        grid.Columns.Add("Asr", "Asr");
        grid.Columns.Add("Maghrib", "Maghrib");
        grid.Columns.Add("Isha", "Isha");
        grid.Columns["Date"]!.Width = 150;

        return grid;
    }

    private static Label CreateLabel(string text) =>
        new()
        {
            Text = text,
            AutoSize = true,
            Font = IslamicTheme.BodyFont(10f, FontStyle.Bold),
            ForeColor = IslamicTheme.Emerald900,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 8)
        };

    private static CheckBox CreatePrayerToggle(string text, bool isChecked)
    {
        var checkBox = new CheckBox
        {
            Text = text,
            Checked = isChecked,
            Margin = new Padding(0, 0, 10, 10)
        };
        IslamicTheme.StylePrayerToggle(checkBox);
        return checkBox;
    }

    private static AppSettings CloneSettings(AppSettings settings)
    {
        return new AppSettings
        {
            City = settings.City,
            Country = settings.Country,
            ReminderLeadMinutes = settings.ReminderLeadMinutes,
            Prayers = new PrayerPreferences
            {
                Fajr = settings.Prayers.Fajr,
                Dhuhr = settings.Prayers.Dhuhr,
                Asr = settings.Prayers.Asr,
                Maghrib = settings.Prayers.Maghrib,
                Isha = settings.Prayers.Isha
            },
            RecentLocations = settings.RecentLocations
                .Select(location => new SavedLocation
                {
                    City = location.City,
                    Country = location.Country
                })
                .ToList()
        };
    }
}
