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

    public void RelayoutToolbar(ToolStrip tStrip)
    {
        if (tStrip.Parent == null) return;
        // Resize and wrap text as necessary
        foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
        {
            btn.Text = _stringWrapper.Wrap(btn.Text ?? "", 80, btn.Font);
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
        ResetToolbarMargin(tStrip);
        // Recalculate sizes
        tStrip.BeginInvoke(() =>
        {
            if (tStrip.Parent.Dock == DockStyle.Top || tStrip.Parent.Dock == DockStyle.Bottom)
            {
                // TODO: If we cache the used width, this check doesn't require any layout - so we can run it on form resize to see if we need to relayout 
                // Check if toolbar buttons are overflowing
                var usedWidth = tStrip.Items.OfType<ToolStripItem>().Select(btn => btn.Width + btn.Margin.Horizontal)
                    .Sum();
                var form = tStrip.FindForm();
                if (form != null && usedWidth > form.Width - FORM_PIXEL_BUFFER)
                {
                    ShrinkToolbarMargin(tStrip);
                }
            }
        });
    }

    private void ResetToolbarMargin(ToolStrip tStrip)
    {
        foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
        {
            if (btn is ToolStripSplitButton)
            {
                if (tStrip.Parent!.Dock == DockStyle.Left || tStrip.Parent.Dock == DockStyle.Right)
                {
                    btn.Margin = new Padding(10, 1, 5, 2);
                }
                else
                {
                    btn.Margin = new Padding(5, 1, 5, 2);
                }
            }
            else if (btn is ToolStripDoubleButton)
            {
                btn.Padding = new Padding(5, 0, 5, 0);
            }
            else if (tStrip.Parent!.Dock == DockStyle.Left || tStrip.Parent.Dock == DockStyle.Right)
            {
                btn.Margin = new Padding(0, 1, 5, 2);
            }
            else
            {
                btn.Padding = new Padding(10, 0, 10, 0);
            }
        }
    }

    private void ShrinkToolbarMargin(ToolStrip tStrip)
    {
        foreach (var btn in tStrip.Items.OfType<ToolStripItem>())
        {
            if (btn is ToolStripSplitButton)
            {
                btn.Margin = new Padding(0, 1, 0, 2);
            }
            else if (btn is ToolStripDoubleButton)
            {
                btn.Padding = new Padding(0, 0, 0, 0);
            }
            else
            {
                btn.Padding = new Padding(5, 0, 5, 0);
            }
        }
    }
}