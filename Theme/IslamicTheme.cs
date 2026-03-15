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
        button.TextAlign = ContentAlignment.MiddleCenter;

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
        button.TextAlign = ContentAlignment.MiddleCenter;
    }

    public static void StylePrayerToggle(CheckBox checkBox)
    {
        checkBox.Appearance = Appearance.Normal;
        checkBox.AutoSize = true;
        checkBox.UseVisualStyleBackColor = false;
        checkBox.BackColor = Color.Transparent;
        checkBox.ForeColor = Ink;
        checkBox.Font = BodyFont(10f);
        checkBox.TextAlign = ContentAlignment.MiddleLeft;
        checkBox.Padding = new Padding(2, 0, 0, 0);
        checkBox.Margin = new Padding(0, 0, 0, 8);
        checkBox.Cursor = Cursors.Hand;
    }
}
