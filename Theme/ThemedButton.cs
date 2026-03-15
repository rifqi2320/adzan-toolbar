using System.Drawing;
using System.Windows.Forms;

namespace AdzanToolbar.Theme;

internal sealed class ThemedButton : Button
{
    public ThemedButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
        UseVisualStyleBackColor = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);

        var rect = ClientRectangle;
        var borderSize = FlatAppearance.BorderSize;
        if (borderSize > 0)
        {
            var borderRect = Rectangle.Inflate(rect, -1, -1);
            using var borderPen = new Pen(FlatAppearance.BorderColor, borderSize);
            e.Graphics.DrawRectangle(borderPen, borderRect);
        }

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            rect,
            Enabled ? ForeColor : SystemColors.GrayText,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis);
    }
}
