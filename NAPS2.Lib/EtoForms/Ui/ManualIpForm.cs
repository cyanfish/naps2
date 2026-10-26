using System.Threading;
using Eto.Forms;
using NAPS2.Escl.Client;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class ManualIpForm : EtoDialogBase
{
    private readonly TextBox _ipHost = new();
    private readonly TextBox _port = new();
    private readonly CheckBox _https = new() { Text = "HTTPS", Checked = true };
    private readonly Spinner _spinner = new() { Enabled = true };
    private readonly ImageView _statusIcon = new();
    private readonly Label _statusLabel = new() { Text = UiStrings.WaitingToConnect };
    private readonly LayoutVisibility _spinnerVis = new(false);
    private readonly LayoutVisibility _statusIconVis = new(false);

    private readonly IIconProvider _iconProvider;

    private CancellationTokenSource? _connectCts;

    public ManualIpForm(Naps2Config config, IIconProvider iconProvider)
        : base(config)
    {
        Title = UiStrings.ManualIpFormTitle;
        IconName = "network_ip";

        _iconProvider = iconProvider;

        EtoPlatform.Current.SetImageSize(_statusIcon, 16);
        EtoPlatform.Current.AttachDpiDependency(this, _ => UpdateStatusIcon());
    }

    public ScanDevice? Device { get; set; }

    public bool Result { get; private set; }

    protected override void BuildLayout()
    {
        FormStateController.RestoreFormState = false;
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.IpHost),
            _ipHost.NaturalWidth(150),
            C.Label(UiStrings.Port),
            L.Row(_port.Width(50), _https),
            C.Spacer(),
            L.Row(
                _spinner.Visible(_spinnerVis).AlignCenter(),
                _statusIcon.Visible(_statusIconVis).AlignCenter(),
                _statusLabel.AlignCenter()
            ),
            C.Filler(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Connect, UiStrings.Connect),
                    C.CancelButton(this))
            )
        );
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _ipHost.Focus();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _connectCts?.Cancel();
    }

    private void UpdateStatusIcon()
    {
        _statusIcon.Image = _iconProvider.GetIcon("exclamation", IconScenario.Inline16, EtoPlatform.Current.GetScaleFactor(this));
    }

    private void SetConnecting()
    {
        _statusIconVis.IsVisible = false;
        _spinnerVis.IsVisible = true;
        _statusLabel.Text = UiStrings.Connecting;
    }

    private void SetError(string message)
    {
        _spinnerVis.IsVisible = false;
        _statusIconVis.IsVisible = true;
        _statusLabel.Text = message;
    }

    private bool Connect()
    {
        string ipHost = _ipHost.Text.Trim();
        if (string.IsNullOrEmpty(ipHost))
        {
            _ipHost.Focus();
            return false;
        }

        string portText = _port.Text.Trim();
        int port = -1;
        if (portText != "" && (!int.TryParse(portText, out port) || port is < 1 or > 65535))
        {
            _port.Focus();
            return false;
        }

        Uri uri;
        try
        {
            uri = new UriBuilder
            {
                Scheme = _https.IsChecked() ? "https" : "http",
                Host = ipHost,
                Port = port,
                Path = "eSCL"
            }.Uri;
        }
        catch (Exception ex) when (ex is UriFormatException or ArgumentException)
        {
            _ipHost.Focus();
            return false;
        }

        // Pressing Connect again restarts the connection with the current input
        _connectCts?.Cancel();
        var cts = new CancellationTokenSource();
        _connectCts = cts;

        SetConnecting();
        Task.Run(async () =>
        {
            try
            {
                var client = new EsclClient(uri) { CancelToken = cts.Token };
                var caps = await client.GetCapabilities();
                if (caps.Uuid != null && caps.MakeAndModel != null)
                {
                    var device = new ScanDevice(Driver.Escl, uri.ToString(), $"{caps.MakeAndModel} ({ipHost})");
                    Invoker.Current.Invoke(() =>
                    {
                        if (cts.IsCancellationRequested) return;
                        Device = device;
                        Result = true;
                        Close();
                    });
                    return;
                }
                Log.Debug($"Missing UUID or make/model in capabilities for manual IP/host {uri}");
                Invoker.Current.Invoke(() =>
                {
                    if (cts.IsCancellationRequested) return;
                    SetError(UiStrings.ConnectionError);
                });
            }
            catch (Exception ex)
            {
                Invoker.Current.Invoke(() =>
                {
                    if (cts.IsCancellationRequested) return;
                    Log.DebugException($"Error connecting to manual IP/host {uri}", ex);
                    bool isTimeout = ex.AnyMatch(e => e is OperationCanceledException or TimeoutException);
                    SetError(isTimeout ? UiStrings.ConnectionTimedOut : UiStrings.ConnectionError);
                });
            }
        });
        return false;
    }
}
