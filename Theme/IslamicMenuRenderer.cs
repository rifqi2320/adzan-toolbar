using System.Drawing;
using System.Windows.Forms;

namespace AdzanToolbar.Theme;

internal sealed class IslamicMenuRenderer : ToolStripProfessionalRenderer
{
    public IslamicMenuRenderer() : base(new IslamicColorTable())
    {
    }

    private sealed class IslamicColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => IslamicTheme.Emerald700;
        public override Color MenuItemBorder => IslamicTheme.Gold500;
        public override Color ToolStripDropDownBackground => IslamicTheme.Parchment;
        public override Color ImageMarginGradientBegin => IslamicTheme.ParchmentDark;
        public override Color ImageMarginGradientMiddle => IslamicTheme.ParchmentDark;
        public override Color ImageMarginGradientEnd => IslamicTheme.ParchmentDark;
        public override Color SeparatorDark => IslamicTheme.Gold500;
        public override Color MenuBorder => IslamicTheme.Gold500;
    }
}
