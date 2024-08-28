using System.Windows.Forms;
using DockStyle = System.Windows.Forms.DockStyle;

namespace NAPS2.WinForms;

/// <summary>
/// Tweaks ToolStrip text and margin to try and avoid or minimize overflow.
/// </summary>
public class ToolbarFormatter
{
    private const int FORM_PIXEL_BUFFER = 30;
    private readonly StringWrapper _stringWrapper;
    private readonly HashSet<char> _charsUsedForAltHotkeys = [];

    public ToolbarFormatter(StringWrapper stringWrapper)
    {
        _stringWrapper = stringWrapper;
    }

    public void RelayoutToolbar(ToolStrip tStrip, float scaleFactor)
    {
        if (tStrip.Parent == null) return;
        // Resize and wrap text as necessary
        foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
        {
            btn.Text = _stringWrapper.Wrap(btn.Text ?? "", (int) (80 * scaleFactor), btn.Font);
            if (!btn.Text.Contains("&"))
            {
                var charToUse = btn.Text.Where(char.IsLetter)
                    .FirstOrDefault(c => !_charsUsedForAltHotkeys.Contains(char.ToLowerInvariant(c)));
                if (charToUse != default)
                {
                    _charsUsedForAltHotkeys.Add(char.ToLowerInvariant(charToUse));
                    var index = btn.Text.IndexOf(charToUse);
                    btn.Text = btn.Text.Substring(0, index) + @"&" + btn.Text.Substring(index);
                }
            }
        }
        ResetToolbarMargin(tStrip, scaleFactor);
        // Recalculate sizes
        Invoker.Current.InvokeDispatch(() =>
        {
            if (tStrip.Parent.Dock == DockStyle.Top || tStrip.Parent.Dock == DockStyle.Bottom)
            {
                // TODO: If we cache the used width, this check doesn't require any layout - so we can run it on form resize to see if we need to relayout 
                // Check if toolbar buttons are overflowing
                var usedWidth = tStrip.Items.OfType<ToolStripItem>().Select(btn => btn.Width + btn.Margin.Horizontal)
                    .Sum();
                var form = tStrip.FindForm();
                if (form != null && usedWidth > form.Width - (int) (FORM_PIXEL_BUFFER * scaleFactor))
                {
                    ShrinkToolbarMargin(tStrip, scaleFactor);
                }
            }
        });
    }

    private void ResetToolbarMargin(ToolStrip tStrip, float scaleFactor)
    {
        int s1 = (int) Math.Round(1 * scaleFactor);
        int s2 = (int) Math.Round(2 * scaleFactor);
        int s5 = (int) Math.Round(5 * scaleFactor);
        int s10 = (int) Math.Round(10 * scaleFactor);
        foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
        {
            if (btn is ToolStripSplitButton)
            {
                if (tStrip.Parent!.Dock == DockStyle.Left || tStrip.Parent.Dock == DockStyle.Right)
                {
                    btn.Margin = new Padding(s10, s1, s5, s2);
                }
                else
                {
                    btn.Margin = new Padding(s5, s1, s5, s2);
                }
            }
            else if (btn is ToolStripDoubleButton)
            {
                btn.Padding = new Padding(s5, 0, s5, 0);
            }
            else if (tStrip.Parent!.Dock == DockStyle.Left || tStrip.Parent.Dock == DockStyle.Right)
            {
                btn.Margin = new Padding(0, s1, s5, s2);
            }
            else
            {
                btn.Padding = new Padding(s10, 0, s10, 0);
            }
        }
    }

    private void ShrinkToolbarMargin(ToolStrip tStrip, float scaleFactor)
    {
        int s1 = (int) Math.Round(1 * scaleFactor);
        int s2 = (int) Math.Round(2 * scaleFactor);
        int s5 = (int) Math.Round(5 * scaleFactor);
        foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
        {
            if (btn is ToolStripSplitButton)
            {
                btn.Margin = new Padding(0, s1, 0, s2);
            }
            else if (btn is ToolStripDoubleButton)
            {
                btn.Padding = new Padding(0, 0, 0, 0);
            }
            else
            {
                btn.Padding = new Padding(s5, 0, s5, 0);
            }
        }
    }
}