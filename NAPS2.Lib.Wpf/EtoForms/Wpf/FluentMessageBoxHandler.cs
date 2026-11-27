using System.Runtime.InteropServices;
using Eto.Forms;
using NAPS2.Platform.Windows;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Shows Eto message boxes with FluentMessageDialog.
/// </summary>
public class FluentMessageBoxHandler : Eto.WidgetHandler<Eto.Widget>, MessageBox.IHandler
{
    public string? Text { get; set; }
    public string? Caption { get; set; }
    public MessageBoxType Type { get; set; }
    public MessageBoxButtons Buttons { get; set; }
    public MessageBoxDefaultButton DefaultButton { get; set; }

    public DialogResult ShowDialog(Control? parent)
    {
        var results = GetResults();
        var cancelResult = GetCancelResult();
        var dialog = new FluentMessageDialog(Caption ?? parent?.ParentWindow?.Title ?? "", Text ?? "", Type,
            results.Select(GetSystemButtonText).ToList(), results.IndexOf(GetDefaultResult()),
            cancelResult != null ? results.IndexOf(cancelResult.Value) : null);
        return results[parent != null ? dialog.ShowModal(parent) : dialog.ShowModal()];
    }

    private List<DialogResult> GetResults() => Buttons switch
    {
        MessageBoxButtons.OKCancel => [DialogResult.Ok, DialogResult.Cancel],
        MessageBoxButtons.YesNo => [DialogResult.Yes, DialogResult.No],
        MessageBoxButtons.YesNoCancel => [DialogResult.Yes, DialogResult.No, DialogResult.Cancel],
        _ => [DialogResult.Ok]
    };

    // Like the native message box, use the Windows display language's button text
    private static string GetSystemButtonText(DialogResult result)
    {
        // MB_GetString takes the button ID - 1 (IDOK = 1, IDCANCEL = 2, IDYES = 6, IDNO = 7)
        uint id = result switch
        {
            DialogResult.Ok => 1,
            DialogResult.Cancel => 2,
            DialogResult.Yes => 6,
            DialogResult.No => 7,
            _ => throw new ArgumentException(nameof(result))
        };
        var text = Marshal.PtrToStringUni(Win32.MB_GetString(id - 1));
        // The "&" access key markers (e.g. "&Yes") become WPF's "_"
        return text?.Replace("_", "__").Replace('&', '_') ?? result.ToString();
    }

    private DialogResult GetDefaultResult() => (DefaultButton, Buttons) switch
    {
        (MessageBoxDefaultButton.OK, MessageBoxButtons.YesNo or MessageBoxButtons.YesNoCancel) => DialogResult.Yes,
        (MessageBoxDefaultButton.OK, _) => DialogResult.Ok,
        (MessageBoxDefaultButton.No, _) => DialogResult.No,
        (MessageBoxDefaultButton.Cancel, _) => DialogResult.Cancel,
        (_, MessageBoxButtons.OKCancel) => DialogResult.Cancel,
        (_, MessageBoxButtons.YesNo) => DialogResult.No,
        (_, MessageBoxButtons.YesNoCancel) => DialogResult.Cancel,
        _ => DialogResult.Ok
    };

    // Like the native message box, Yes/No can't be dismissed without choosing one
    private DialogResult? GetCancelResult() => Buttons switch
    {
        MessageBoxButtons.OK => DialogResult.Ok,
        MessageBoxButtons.YesNo => null,
        _ => DialogResult.Cancel
    };
}
