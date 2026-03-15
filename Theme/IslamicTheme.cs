using System.Drawing;
using System.Windows.Forms;

namespace AdzanToolbar.Theme;

internal static class IslamicTheme
{
    public static Color Emerald900 => Color.FromArgb(19, 58, 52);
    public static Color Emerald700 => Color.FromArgb(32, 93, 82);
    public static Color Emerald500 => Color.FromArgb(62, 132, 108);
    public static Color Lapis700 => Color.FromArgb(34, 74, 120);
    public static Color Gold500 => Color.FromArgb(198, 156, 74);
    public static Color Gold300 => Color.FromArgb(231, 205, 142);
    public static Color Parchment => Color.FromArgb(245, 238, 220);
    public static Color ParchmentDark => Color.FromArgb(229, 219, 190);
    public static Color Sand => Color.FromArgb(237, 229, 206);
    public static Color Ink => Color.FromArgb(41, 41, 36);
    public static Color Rosewood => Color.FromArgb(111, 66, 50);
    public static Color Slate => Color.FromArgb(79, 86, 92);
    public static Color TodayHighlight => Color.FromArgb(219, 194, 136);

    public static Font HeaderFont(float size, FontStyle style = FontStyle.Bold) =>
        new("Palatino Linotype", size, style, GraphicsUnit.Point);

    public static Font BodyFont(float size, FontStyle style = FontStyle.Regular) =>
        new("Segoe UI", size, style, GraphicsUnit.Point);

    public static void StyleInput(Control control)
    {
        control.BackColor = Color.White;
        control.ForeColor = Ink;
        control.Font = BodyFont(10.5f);
        control.Margin = new Padding(0, 4, 0, 12);
    }

    public static void StyleButton(Button button, bool primary)
    {
        button.AutoSize = false;
        button.Height = 42;
        button.Width = 110;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
        button.Font = BodyFont(10.5f, FontStyle.Bold);

        if (primary)
        {
            button.BackColor = Emerald700;
            button.ForeColor = Parchment;
        }
        else
        {
            button.BackColor = ParchmentDark;
            button.ForeColor = Ink;
        }
    }

    public static void StyleFlatActionButton(Button button)
    {
        button.AutoSize = false;
        button.Height = 36;
        button.Width = 112;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Gold500;
        button.BackColor = Color.White;
        button.ForeColor = Emerald900;
        button.Cursor = Cursors.Hand;
        button.Font = BodyFont(10f, FontStyle.Bold);
    }

    public static void StylePrayerToggle(CheckBox checkBox)
    {
        checkBox.Appearance = Appearance.Button;
        checkBox.AutoSize = false;
        checkBox.TextAlign = ContentAlignment.MiddleCenter;
        checkBox.Size = new Size(96, 38);
        checkBox.FlatStyle = FlatStyle.Flat;
        checkBox.FlatAppearance.BorderSize = 1;
        checkBox.FlatAppearance.BorderColor = Gold500;
        checkBox.Font = BodyFont(10f, FontStyle.Bold);
        checkBox.Cursor = Cursors.Hand;

        void RefreshState()
        {
            checkBox.BackColor = checkBox.Checked ? Emerald700 : Color.White;
            checkBox.ForeColor = checkBox.Checked ? Parchment : Emerald900;
        }

        checkBox.CheckedChanged += (_, _) => RefreshState();
        RefreshState();
    }
}
