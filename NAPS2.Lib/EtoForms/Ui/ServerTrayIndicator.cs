using Eto.Forms;

namespace NAPS2.EtoForms.Ui;

public class ServerTrayIndicator : TrayIndicator
{
    public ServerTrayIndicator()
    {
        // TODO: Maybe use a higher-quality icon with hidpi?
        Image = EtoPlatform.Current.IsWinForms
            ? Icons.scanner_16.ToEtoImage() // Windows has small tray icons
            : EtoPlatform.Current.IsMac
                ? Icons.scanner_gray_32.ToEtoImage() // Gray to match macOS tray style
                : Icons.scanner_32.ToEtoImage();
        Title = string.Format(UiStrings.Naps2TitleFormat, UiStrings.ScannerSharing);
        Menu = new ContextMenu(
            new ButtonMenuItem
            {
                Text = UiStrings.StopScannerSharing,
                Command = new ActionCommand(() => StopClicked?.Invoke(this, EventArgs.Empty))
            }
        );
    }

    public event EventHandler? StopClicked;
}