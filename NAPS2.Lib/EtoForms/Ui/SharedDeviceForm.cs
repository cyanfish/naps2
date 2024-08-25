using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.EtoForms.Ui;

public class SharedDeviceForm : EtoDialogBase
{
    private const int BASE_PORT = 9801;
    private const int BASE_TLS_PORT = 9901;

    private readonly ErrorOutput _errorOutput;
    private readonly ISharedDeviceManager _sharedDeviceManager;

    private readonly TextBox _displayName = new();
    private readonly DeviceSelectorWidget _deviceSelectorWidget;

    public SharedDeviceForm(Naps2Config config, IScanPerformer scanPerformer, ErrorOutput errorOutput,
        ISharedDeviceManager sharedDeviceManager, DeviceCapsCache deviceCapsCache,
        IIconProvider iconProvider) : base(config)
    {
        Title = UiStrings.SharedDeviceFormTitle;
        Icon = new Icon(1f, iconProvider.GetIcon("wireless16"));

        _errorOutput = errorOutput;
        _sharedDeviceManager = sharedDeviceManager;
        _deviceSelectorWidget = new(scanPerformer, deviceCapsCache, iconProvider, this)
        {
            ProfileFunc = () => new ScanProfile { DriverName = DeviceDriver.ToString().ToLowerInvariant() },
            AllowAlwaysAsk = false
        };
        _deviceSelectorWidget.DeviceChanged += DeviceChanged;
    }

    private void DeviceChanged(object? sender, DeviceChangedEventArgs e)
    {
        if (e.NewChoice.Device != null && (string.IsNullOrEmpty(_displayName.Text) ||
                                           e.PreviousChoice.Device?.Name == _displayName.Text))
        {
            _displayName.Text = e.NewChoice.Device.Name;
        }
        DeviceDriver = e.NewChoice.Driver;
    }

    protected override void BuildLayout()
    {
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DisplayNameLabel),
            _displayName,
            C.Spacer(),
            _deviceSelectorWidget,
            C.Filler(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, SaveSettings),
                    C.CancelButton(this))
            )
        );
    }

    public bool Result { get; private set; }

    public SharedDevice? SharedDevice { get; set; }

    private int Port { get; set; }

    private int TlsPort { get; set; }

    private Driver DeviceDriver { get; set; } = ScanOptionsValidator.SystemDefaultDriver;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (SharedDevice != null)
        {
            _displayName.Text = SharedDevice.Name;
            _deviceSelectorWidget.Choice = DeviceChoice.ForDevice(SharedDevice.Device);
            Port = SharedDevice.Port;
            TlsPort = SharedDevice.TlsPort;
            DeviceDriver = SharedDevice.Device.Driver;
        }
    }

    private bool SaveSettings()
    {
        if (_displayName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return false;
        }
        if (_deviceSelectorWidget.Choice.Device == null)
        {
            _errorOutput.DisplayError(MiscResources.NoDeviceSelected);
            return false;
        }
        SharedDevice = new SharedDevice
        {
            Name = _displayName.Text,
            Device = _deviceSelectorWidget.Choice.Device,
            Port = Port == 0 ? NextPort() : Port,
            TlsPort = TlsPort == 0 ? NextTlsPort() : TlsPort
        };
        Result = true;
        return true;
    }

    private int NextPort()
    {
        var devices = _sharedDeviceManager.SharedDevices;
        int port = BASE_PORT;
        while (devices.Any(x => x.Port == port))
        {
            port++;
        }
        return port;
    }

    private int NextTlsPort()
    {
        var devices = _sharedDeviceManager.SharedDevices;
        int tlsPort = BASE_TLS_PORT;
        while (devices.Any(x => x.TlsPort == tlsPort))
        {
            tlsPort++;
        }
        return tlsPort;
    }
}