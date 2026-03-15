using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    private ComboBox _countryComboBox = null!;
    private ComboBox _cityComboBox = null!;
    private NumericUpDown _leadMinutesNumeric = null!;
    private CheckBox _fajrCheckBox = null!;
    private CheckBox _dhuhrCheckBox = null!;
    private CheckBox _asrCheckBox = null!;
    private CheckBox _maghribCheckBox = null!;
    private CheckBox _ishaCheckBox = null!;
    private DataGridView _weekGrid = null!;
    private Label _statusLabel = null!;
    private Button _refreshButton = null!;

    public SettingsForm(AppSettings settings, PrayerScheduleRepository scheduleRepository)
    {
        SuspendLayout();
        _settings = CloneSettings(settings);
        _scheduleRepository = scheduleRepository;

        Text = "Adzan Toolbar Settings";
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(980, 710);
        BackColor = IslamicTheme.Parchment;
        Font = IslamicTheme.BodyFont(10f);
        Padding = new Padding(1);
        SizeGripStyle = SizeGripStyle.Hide;

        _windowIcon = TrayIconFactory.Create();
        Icon = _windowIcon;
        FormClosed += (_, _) => _windowIcon.Dispose();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = IslamicTheme.Parchment
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));

        var header = new IslamicHeaderPanel
        {
            TitleText = "Adzan Companion",
            SubtitleText = "Prayer schedule and reminder controls"
        };
        header.Height = 150;

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(18, 16, 18, 12),
            BackColor = IslamicTheme.Parchment
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 332));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var settingsCard = CreateCard();
        settingsCard.Padding = new Padding(16, 14, 16, 14);
        settingsCard.Margin = new Padding(0, 0, 14, 0);
        settingsCard.Controls.Add(BuildSettingsPane());

        var scheduleCard = CreateCard();
        scheduleCard.Padding = new Padding(16, 14, 16, 14);
        scheduleCard.Margin = new Padding(0);
        scheduleCard.Controls.Add(BuildSchedulePane());

        content.Controls.Add(settingsCard, 0, 0);
        content.Controls.Add(scheduleCard, 1, 0);

        var footer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 8, 18, 14),
            BackColor = IslamicTheme.Parchment
        };
        footer.Controls.Add(BuildFooterButtons());

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(content, 0, 1);
        root.Controls.Add(footer, 0, 2);
        Controls.Add(root);

        ResumeLayout(performLayout: true);

        Shown += async (_, _) =>
        {
            RefreshCitySuggestions();
            await LoadWeekAsync().ConfigureAwait(true);
        };
    }

    public event EventHandler<AppSettings>? SettingsSaved;

    private Control BuildSettingsPane()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent
        };
        panel.SuspendLayout();

        _countryComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.ListItems,
            Dock = DockStyle.Top
        };
        _countryComboBox.Items.AddRange(LocationCatalog.GetCountries().Cast<object>().ToArray());
        _countryComboBox.Text = _settings.Country;
        _countryComboBox.SelectedIndexChanged += (_, _) => RefreshCitySuggestions();
        _countryComboBox.TextChanged += (_, _) => RefreshCitySuggestions();
        IslamicTheme.StyleInput(_countryComboBox);

        _cityComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.CustomSource,
            Dock = DockStyle.Top
        };
        _cityComboBox.Text = _settings.City;
        IslamicTheme.StyleInput(_cityComboBox);

        _leadMinutesNumeric = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 120,
            Value = _settings.ReminderLeadMinutes,
            Dock = DockStyle.Left,
            Width = 120
        };
        IslamicTheme.StyleInput(_leadMinutesNumeric);

        _fajrCheckBox = CreatePrayerToggle("Fajr", _settings.Prayers.Fajr);
        _dhuhrCheckBox = CreatePrayerToggle("Dhuhr", _settings.Prayers.Dhuhr);
        _asrCheckBox = CreatePrayerToggle("Asr", _settings.Prayers.Asr);
        _maghribCheckBox = CreatePrayerToggle("Maghrib", _settings.Prayers.Maghrib);
        _ishaCheckBox = CreatePrayerToggle("Isha", _settings.Prayers.Isha);

        var prayerPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 4, 0, 8),
            BackColor = Color.Transparent
        };
        prayerPanel.Controls.AddRange(
            [_fajrCheckBox, _dhuhrCheckBox, _asrCheckBox, _maghribCheckBox, _ishaCheckBox]);

        _refreshButton = new Button
        {
            Text = "Refresh Week",
            Anchor = AnchorStyles.Left
        };
        IslamicTheme.StyleFlatActionButton(_refreshButton);
        _refreshButton.Click += async (_, _) => await LoadWeekAsync().ConfigureAwait(true);

        var metaLabel = new Label
        {
            Text = "Method 20 · Kemenag Indonesia · 30 sec refresh",
            AutoSize = true,
            ForeColor = IslamicTheme.Slate,
            Font = IslamicTheme.BodyFont(9.5f),
            Margin = new Padding(0, 10, 0, 0)
        };

        var locationSection = CreateGroupSection("Location");
        locationSection.Controls.Add(CreateField("City", _cityComboBox));
        locationSection.Controls.Add(CreateField("Country", _countryComboBox));

        var reminderSection = CreateGroupSection("Reminder");
        reminderSection.Controls.Add(CreateField("Lead Minutes", _leadMinutesNumeric));

        var prayerSection = CreateGroupSection("Prayers");
        prayerSection.Controls.Add(prayerPanel);

        var runtimeSection = CreateGroupSection("Runtime");
        var actionRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 4, 0, 0)
        };
        actionRow.Controls.Add(_refreshButton);
        actionRow.Controls.Add(metaLabel);
        runtimeSection.Controls.Add(actionRow);

        panel.Controls.Add(locationSection);
        panel.Controls.Add(reminderSection);
        panel.Controls.Add(prayerSection);
        panel.Controls.Add(runtimeSection);
        panel.ResumeLayout(performLayout: true);
        return panel;
    }

    private Control BuildSchedulePane()
    {
        var panel = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Prayer Week",
            Font = IslamicTheme.BodyFont(10f, FontStyle.Bold),
            ForeColor = IslamicTheme.Emerald900,
            BackColor = Color.Transparent,
            Padding = new Padding(14, 14, 14, 14)
        };
        panel.SuspendLayout();

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _statusLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            ForeColor = IslamicTheme.Rosewood,
            Font = IslamicTheme.BodyFont(9.5f),
            Margin = new Padding(0, 6, 0, 10)
        };
        content.Controls.Add(_statusLabel, 0, 0);

        _weekGrid = CreateWeekGrid();
        content.Controls.Add(_weekGrid, 0, 1);
        panel.Controls.Add(content);
        panel.ResumeLayout(performLayout: true);
        return panel;
    }

    private Control BuildFooterButtons()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 260,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var saveButton = new Button { Text = "Save" };
        IslamicTheme.StyleButton(saveButton, primary: true);
        saveButton.Click += (_, _) => SaveAndClose();

        var cancelButton = new Button { Text = "Cancel" };
        IslamicTheme.StyleButton(cancelButton, primary: false);
        cancelButton.Click += (_, _) => Close();

        panel.Controls.Add(saveButton);
        panel.Controls.Add(cancelButton);
        return panel;
    }

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
            ToggleLoading(true, "Loading prayer week...");
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
        var today = DateOnly.FromDateTime(DateTime.Today);

        foreach (var day in week)
        {
            var rowIndex = _weekGrid.Rows.Add(
                day.Date.ToString("ddd, dd MMM"),
                day.Fajr,
                day.Dhuhr,
                day.Asr,
                day.Maghrib,
                day.Isha);

            var row = _weekGrid.Rows[rowIndex];
            row.Tag = day.Date;

            if (day.Date == today)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(247, 240, 220);
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 240, 220);
                row.DefaultCellStyle.ForeColor = IslamicTheme.Ink;
                row.DefaultCellStyle.SelectionForeColor = IslamicTheme.Ink;
            }
        }

        _weekGrid.ClearSelection();
        _weekGrid.Invalidate();
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
            AllowUserToResizeColumns = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            GridColor = IslamicTheme.ParchmentDark,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 42 },
            ColumnHeadersHeight = 40,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = IslamicTheme.Emerald700,
                ForeColor = IslamicTheme.Parchment,
                Font = IslamicTheme.BodyFont(10f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = IslamicTheme.Ink,
                SelectionBackColor = Color.White,
                SelectionForeColor = IslamicTheme.Ink,
                Font = IslamicTheme.BodyFont(10f),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(6, 0, 6, 0)
            }
        };

        grid.Columns.Add(CreateColumn("Date", "Date", 148));
        grid.Columns.Add(CreateColumn("Fajr", "Fajr"));
        grid.Columns.Add(CreateColumn("Dhuhr", "Dhuhr"));
        grid.Columns.Add(CreateColumn("Asr", "Asr"));
        grid.Columns.Add(CreateColumn("Maghrib", "Maghrib"));
        grid.Columns.Add(CreateColumn("Isha", "Isha"));

        grid.RowPostPaint += (_, e) =>
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (grid.Rows[e.RowIndex].Tag is not DateOnly date || date != DateOnly.FromDateTime(DateTime.Today))
            {
                return;
            }

            var bounds = new Rectangle(
                grid.RowHeadersWidth,
                e.RowBounds.Top,
                grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) - 1,
                e.RowBounds.Height - 1);

            using var pen = new Pen(IslamicTheme.TodayHighlight, 2f);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRectangle(pen, bounds);
        };

        return grid;
    }

    private static DataGridViewTextBoxColumn CreateColumn(string name, string headerText, int width = 0)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = headerText,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Width = width > 0 ? width : 108,
            AutoSizeMode = width > 0 ? DataGridViewAutoSizeColumnMode.None : DataGridViewAutoSizeColumnMode.Fill
        };
    }

    private static Panel CreateField(string labelText, Control input)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = input is NumericUpDown ? 74 : 78,
            BackColor = Color.Transparent
        };

        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Height = 24,
            ForeColor = IslamicTheme.Emerald900,
            Font = IslamicTheme.BodyFont(9.5f, FontStyle.Bold)
        };

        input.Dock = DockStyle.Bottom;
        panel.Controls.Add(input);
        panel.Controls.Add(label);
        return panel;
    }

    private static GroupBox CreateGroupSection(string text)
    {
        var group = new GroupBox
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Width = 270,
            Padding = new Padding(14, 16, 14, 12),
            Margin = new Padding(0, 0, 0, 12),
            ForeColor = IslamicTheme.Rosewood,
            Font = IslamicTheme.BodyFont(10f, FontStyle.Bold),
            BackColor = Color.Transparent
        };
        return group;
    }

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

    private static CardPanel CreateCard()
    {
        return new CardPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(250, 246, 236)
        };
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

    private sealed class CardPanel : Panel
    {
        public CardPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var brush = new SolidBrush(BackColor);
            using var borderPen = new Pen(Color.FromArgb(195, IslamicTheme.Gold300), 1f);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillRectangle(brush, rect);
            e.Graphics.DrawRectangle(borderPen, rect);
        }
    }
}
