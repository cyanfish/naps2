namespace NAPS2.WinForms;

using wf = System.Windows.Forms;

public static class WinFormsExtensions
{
    public static wf.MessageBoxIcon ToWinForms(this MessageBoxIcon icon)
    {
        return icon switch
        {
            MessageBoxIcon.Information => wf.MessageBoxIcon.Information,
            MessageBoxIcon.Warning => wf.MessageBoxIcon.Warning,
            _ => wf.MessageBoxIcon.None
        };
    }

    public static wf.DockStyle ToWinForms(this DockStyle dock)
    {
        return dock switch
        {
            DockStyle.Bottom => wf.DockStyle.Bottom,
            DockStyle.Left => wf.DockStyle.Left,
            DockStyle.Right => wf.DockStyle.Right,
            _ => wf.DockStyle.Top
        };
    }

    public static DockStyle ToConfig(this wf.DockStyle dock)
    {
        return dock switch
        {
            wf.DockStyle.Bottom => DockStyle.Bottom,
            wf.DockStyle.Left => DockStyle.Left,
            wf.DockStyle.Right => DockStyle.Right,
            _ => DockStyle.Top
        };
    }
}