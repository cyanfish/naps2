using Eto.Drawing;
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

    private readonly ErrorOutput _errorOutput;

    public ManualIpForm(Naps2Config config, ErrorOutput errorOutput, IIconProvider iconProvider)
        : base(config)
    {
        Title = UiStrings.ManualIpFormTitle;
        IconName = "network_ip_small";

        _errorOutput = errorOutput;
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

    private bool Connect()
    {
        string ipHost = _ipHost.Text;
        if (string.IsNullOrWhiteSpace(ipHost))
        {
            _ipHost.Focus();
            return false;
        }

        int port = int.TryParse(_port.Text, out int p) ? p : -1;
        bool https = _https.IsChecked();

        Task.Run(async () =>
        {
            var uri = new UriBuilder
            {
                Scheme = https ? "https" : "http",
                Host = ipHost,
                Port = port,
                Path = "eSCL"
            }.Uri;

            try
            {
                var client = new EsclClient(uri);
                var caps = await client.GetCapabilities();
                if (caps.Uuid != null && caps.MakeAndModel != null)
                {
                    Device = new ScanDevice(Driver.Escl, uri.ToString(), $"{caps.MakeAndModel} ({ipHost})");
                    Result = true;
                    Invoker.Current.Invoke(Close);
                }
            }
            catch (Exception ex)
            {
                Log.DebugException($"Error connecting to manual IP/host {uri.ToString()}", ex);
                _errorOutput.DisplayError(UiStrings.ConnectionError, ex);
            }
        });
        return false;
    }
}