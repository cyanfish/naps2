using System.Windows.Forms;

namespace NAPS2.WinForms;

/// <summary>
/// A basic implementation of an invisible form.
/// </summary>
public class BackgroundForm : Form
{
    public BackgroundForm()
    {
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        // Even though the form is invisible, we want to make sure it's positioned inside
        // its parent (if present) so any child forms are also inside the parent.
        StartPosition = FormStartPosition.CenterParent;
    }

    protected override void SetVisibleCore(bool value)
    {
        if (!IsHandleCreated)
        {
            CreateHandle();
        }
        value = false;
        base.SetVisibleCore(value);
    }
}