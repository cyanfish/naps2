namespace NAPS2.WinForms;

using WF = System.Windows.Forms;

public static class WinFormsExtensions
{
    public static WF.MessageBoxIcon ToWinForms(this MessageBoxIcon icon)
    {
        return icon switch
        {
            MessageBoxIcon.Information => WF.MessageBoxIcon.Information,
            MessageBoxIcon.Warning => WF.MessageBoxIcon.Warning,
            _ => WF.MessageBoxIcon.None
        };
    }

    public static WF.DockStyle ToWinForms(this DockStyle dock)
    {
        return dock switch
        {
            DockStyle.Bottom => WF.DockStyle.Bottom,
            DockStyle.Left => WF.DockStyle.Left,
            DockStyle.Right => WF.DockStyle.Right,
            _ => WF.DockStyle.Top
        };
    }

    public static DockStyle ToConfig(this WF.DockStyle dock)
    {
        return dock switch
        {
            WF.DockStyle.Bottom => DockStyle.Bottom,
            WF.DockStyle.Left => DockStyle.Left,
            WF.DockStyle.Right => DockStyle.Right,
            _ => DockStyle.Top
        };
    }
}