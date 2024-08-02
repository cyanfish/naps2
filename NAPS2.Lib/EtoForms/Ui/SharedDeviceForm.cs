using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Remoting.Server;
using NAPS2.Scan;
using NAPS2.Scan.Internal;

namespace NAPS2.EtoForms.Ui;

public class SharedDeviceForm : EtoDialogBase
{
    private const int BASE_PORT = 9801;
    private const int BASE_TLS_PORT = 9901;

    private readonly IScanPerformer _scanPerformer;
    private readonly ErrorOutput _errorOutput;
    private readonly ISharedDeviceManager _sharedDeviceManager;

    private readonly TextBox _displayName = new();
    private readonly RadioButton _wiaDriver;
    private readonly RadioButton _twainDriver;
    private readonly RadioButton _appleDriver;
    private readonly RadioButton _saneDriver;
    private readonly TextBox _deviceName = new() { Enabled = false };
    private readonly Button _chooseDevice = new() { Text = UiStrings.ChooseDevice };
    private readonly Button _ok = new() { Text = UiStrings.OK };
    private readonly Button _cancel = new() { Text = UiStrings.Cancel };

    private ScanDevice? _currentDevice;
    private bool _result;
    private bool _suppressChangeEvent;

    public SharedDeviceForm(Naps2Config config, IScanPerformer scanPerformer, ErrorOutput errorOutput,
        ISharedDeviceManager sharedDeviceManager) : base(config)
    {
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _sharedDeviceManager = sharedDeviceManager;
        _wiaDriver = new RadioButton { Text = UiStrings.WiaDriver };
        _twainDriver = new RadioButton(_wiaDriver) { Text = UiStrings.TwainDriver };
        _appleDriver = new RadioButton(_wiaDriver) { Text = UiStrings.AppleDriver };
        _saneDriver = new RadioButton(_wiaDriver) { Text = UiStrings.SaneDriver };
        _wiaDriver.CheckedChanged += Driver_CheckedChanged;
        _twainDriver.CheckedChanged += Driver_CheckedChanged;
        _appleDriver.CheckedChanged += Driver_CheckedChanged;
        _saneDriver.CheckedChanged += Driver_CheckedChanged;
        _ok.Click += Ok_Click;
        _cancel.Click += Cancel_Click;

        _chooseDevice.Click += ChooseDevice;
        _deviceName.KeyDown += DeviceName_KeyDown;
    }

    protected override void BuildLayout()
    {
        // TODO: Don't show if only one driver is available
        var driverElements = new List<LayoutElement>();
        if (PlatformCompat.System.IsWiaDriverSupported)
        {
            driverElements.Add(_wiaDriver.Scale());
        }
        if (PlatformCompat.System.IsTwainDriverSupported)
        {
            driverElements.Add(_twainDriver.Scale());
        }
        if (PlatformCompat.System.IsAppleDriverSupported)
        {
            driverElements.Add(_appleDriver.Scale());
        }
        if (PlatformCompat.System.IsSaneDriverSupported)
        {
            driverElements.Add(_saneDriver.Scale());
        }

        Title = UiStrings.SharedDeviceFormTitle;
        Icon = new Icon(1f, Icons.wireless16.ToEtoImage());

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            L.Row(
                L.Column(
                    C.Label(UiStrings.DisplayNameLabel),
                    _displayName,
                    L.Row(
                        driverElements.ToArray()
                    ),
                    C.Spacer(),
                    C.Label(UiStrings.DeviceLabel),
                    L.Row(
                        _deviceName.Scale(),
                        _chooseDevice
                    )
                ).Scale(),
                new ImageView { Image = Icons.scanner_48.ToEtoImage() }
            ),
            C.Filler(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    _ok,
                    _cancel)
            )
        );
    }

    public bool Result => _result;

    public SharedDevice? SharedDevice { get; set; }

    public ScanDevice? CurrentDevice
    {
        get => _currentDevice;
        set
        {
            _currentDevice = value;
            _deviceName.Text = value?.Name ?? "";
        }
    }

    private int Port { get; set; }

    private int TlsPort { get; set; }

    private Driver DeviceDriver
    {
        get => _twainDriver.Checked ? Driver.Twain
            : _wiaDriver.Checked ? Driver.Wia
            : _appleDriver.Checked ? Driver.Apple
            : _saneDriver.Checked ? Driver.Sane
            : ScanOptionsValidator.SystemDefaultDriver;
        set
        {
            if (value == Driver.Twain)
            {
                _twainDriver.Checked = true;
            }
            else if (value == Driver.Wia)
            {
                _wiaDriver.Checked = true;
            }
            else if (value == Driver.Apple)
            {
                _appleDriver.Checked = true;
            }
            else if (value == Driver.Sane)
            {
                _saneDriver.Checked = true;
            }
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Don't trigger any onChange events
        _suppressChangeEvent = true;

        _displayName.Text = SharedDevice?.Name ?? "";
        CurrentDevice ??= SharedDevice?.Device;
        Port = SharedDevice?.Port ?? 0;
        TlsPort = SharedDevice?.TlsPort ?? 0;

        DeviceDriver = SharedDevice?.Device.Driver ?? ScanOptionsValidator.SystemDefaultDriver;

        // Start triggering onChange events again
        _suppressChangeEvent = false;
    }

    private async void ChooseDevice(object? sender, EventArgs args)
    {
        var profile = new ScanProfile { DriverName = DeviceDriver.ToString().ToLowerInvariant() };
        var device = (await _scanPerformer.PromptForDevice(profile, false, NativeHandle)).Device;
        if (device != null)
        {
            if (string.IsNullOrEmpty(_displayName.Text) ||
                CurrentDevice != null && CurrentDevice.Name == _displayName.Text)
            {
                _displayName.Text = device.Name;
            }
            CurrentDevice = device;
        }
    }

    private void SaveSettings()
    {
        SharedDevice = new SharedDevice
        {
            Name = _displayName.Text,
            Device = CurrentDevice!,
            Port = Port == 0 ? NextPort() : Port,
            TlsPort = TlsPort == 0 ? NextTlsPort() : TlsPort
        };
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

    private void Ok_Click(object? sender, EventArgs e)
    {
        if (_displayName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return;
        }
        if (CurrentDevice == null)
        {
            _errorOutput.DisplayError(MiscResources.NoDeviceSelected);
            return;
        }
        _result = true;
        SaveSettings();
        Close();
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void Driver_CheckedChanged(object? sender, EventArgs e)
    {
        if (((RadioButton) sender!).Checked && !_suppressChangeEvent)
        {
            SharedDevice = null;
            CurrentDevice = null;
        }
    }

    private void DeviceName_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Keys.Delete)
        {
            CurrentDevice = null;
        }
    }
}