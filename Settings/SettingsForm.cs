using System;
using System.Drawing;
using System.Windows.Forms;
using AdzanToolbar.Theme;
using AdzanToolbar.Tray;

namespace AdzanToolbar.Settings;

internal sealed class SettingsForm : Form
{
    private readonly Icon _windowIcon;
    private readonly TextBox _cityTextBox;
    private readonly TextBox _countryTextBox;
    private readonly NumericUpDown _methodNumeric;
    private readonly NumericUpDown _leadMinutesNumeric;
    private readonly NumericUpDown _pollingSecondsNumeric;
    private readonly CheckBox _fajrCheckBox;
    private readonly CheckBox _dhuhrCheckBox;
    private readonly CheckBox _asrCheckBox;
    private readonly CheckBox _maghribCheckBox;
    private readonly CheckBox _ishaCheckBox;

    public SettingsForm(AppSettings settings)
    {
        Text = "Adzan Toolbar Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(560, 640);
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
            SubtitleText = "Emerald, gold, and geometric motifs inspired by Islamic art and architecture"
        };

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 14, 18, 18),
            BackColor = IslamicTheme.Parchment,
            AutoScroll = true
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 11,
            BackColor = IslamicTheme.Parchment
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _cityTextBox = new TextBox { Text = settings.City, Dock = DockStyle.Fill };
        _countryTextBox = new TextBox { Text = settings.Country, Dock = DockStyle.Fill };
        _methodNumeric = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 30,
            Value = settings.CalculationMethod,
            Dock = DockStyle.Left,
            Width = 100
        };
        _leadMinutesNumeric = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 120,
            Value = settings.ReminderLeadMinutes,
            Dock = DockStyle.Left,
            Width = 100
        };
        _pollingSecondsNumeric = new NumericUpDown
        {
            Minimum = 10,
            Maximum = 300,
            Value = settings.PollingIntervalSeconds,
            Dock = DockStyle.Left,
            Width = 100
        };

        _fajrCheckBox = CreatePrayerToggle("Fajr", settings.Prayers.Fajr);
        _dhuhrCheckBox = CreatePrayerToggle("Dhuhr", settings.Prayers.Dhuhr);
        _asrCheckBox = CreatePrayerToggle("Asr", settings.Prayers.Asr);
        _maghribCheckBox = CreatePrayerToggle("Maghrib", settings.Prayers.Maghrib);
        _ishaCheckBox = CreatePrayerToggle("Isha", settings.Prayers.Isha);

        IslamicTheme.StyleInput(_cityTextBox);
        IslamicTheme.StyleInput(_countryTextBox);
        IslamicTheme.StyleInput(_methodNumeric);
        IslamicTheme.StyleInput(_leadMinutesNumeric);
        IslamicTheme.StyleInput(_pollingSecondsNumeric);

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

        var introCard = CreateCard();
        introCard.Controls.Add(new Label
        {
            Text = "Set your city and country, keep KEMENAG method 20 by default, and choose which prayers should notify from the tray.",
            Dock = DockStyle.Fill,
            Font = IslamicTheme.BodyFont(10f),
            ForeColor = IslamicTheme.Ink,
            AutoSize = false
        });
        layout.SetColumnSpan(introCard, 2);
        layout.Controls.Add(introCard, 0, 0);

        layout.Controls.Add(CreateLabel("City"), 0, 1);
        layout.Controls.Add(_cityTextBox, 1, 1);
        layout.Controls.Add(CreateLabel("Country"), 0, 2);
        layout.Controls.Add(_countryTextBox, 1, 2);
        layout.Controls.Add(CreateLabel("Calculation Method"), 0, 3);
        layout.Controls.Add(_methodNumeric, 1, 3);
        layout.Controls.Add(CreateLabel("Reminder Lead Minutes"), 0, 4);
        layout.Controls.Add(_leadMinutesNumeric, 1, 4);
        layout.Controls.Add(CreateLabel("Polling Seconds"), 0, 5);
        layout.Controls.Add(_pollingSecondsNumeric, 1, 5);
        layout.Controls.Add(CreateLabel("Enabled Prayers"), 0, 6);
        layout.Controls.Add(prayersPanel, 1, 6);

        var helpLabel = new Label
        {
            Text = "Method 20 matches KEMENAG Indonesia. Lead minutes lets you notify before adhan if needed.",
            Dock = DockStyle.Fill,
            AutoSize = true,
            Font = IslamicTheme.BodyFont(9.5f),
            ForeColor = IslamicTheme.Rosewood,
            Margin = new Padding(0, 6, 0, 12)
        };
        layout.SetColumnSpan(helpLabel, 2);
        layout.Controls.Add(helpLabel, 0, 7);

        var footerCard = CreateCard();
        footerCard.Controls.Add(new Label
        {
            Text = "Visual direction: emerald and gold with geometric motifs, inspired by Islamic tiles, manuscripts, and architectural interiors.",
            Dock = DockStyle.Fill,
            Font = IslamicTheme.BodyFont(9.5f),
            ForeColor = IslamicTheme.Ink
        });
        layout.SetColumnSpan(footerCard, 2);
        layout.Controls.Add(footerCard, 0, 8);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 10, 0, 0)
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
        layout.SetColumnSpan(buttonPanel, 2);
        layout.Controls.Add(buttonPanel, 0, 9);

        contentPanel.Controls.Add(layout);
        root.Controls.Add(contentPanel);
        root.Controls.Add(header);
        Controls.Add(root);
    }

    public event EventHandler<AppSettings>? SettingsSaved;

    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(_cityTextBox.Text) || string.IsNullOrWhiteSpace(_countryTextBox.Text))
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
            City = _cityTextBox.Text.Trim(),
            Country = _countryTextBox.Text.Trim(),
            CalculationMethod = Decimal.ToInt32(_methodNumeric.Value),
            ReminderLeadMinutes = Decimal.ToInt32(_leadMinutesNumeric.Value),
            PollingIntervalSeconds = Decimal.ToInt32(_pollingSecondsNumeric.Value),
            Prayers = new PrayerPreferences
            {
                Fajr = _fajrCheckBox.Checked,
                Dhuhr = _dhuhrCheckBox.Checked,
                Asr = _asrCheckBox.Checked,
                Maghrib = _maghribCheckBox.Checked,
                Isha = _ishaCheckBox.Checked
            }
        };

        SettingsSaved?.Invoke(this, settings);
        Close();
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

    private static Panel CreateCard()
    {
        return new Panel
        {
            Height = 72,
            Dock = DockStyle.Fill,
            Padding = new Padding(14, 12, 14, 12),
            Margin = new Padding(0, 0, 0, 14),
            BackColor = Color.FromArgb(250, 246, 236),
            BorderStyle = BorderStyle.FixedSingle
        };
    }
}
