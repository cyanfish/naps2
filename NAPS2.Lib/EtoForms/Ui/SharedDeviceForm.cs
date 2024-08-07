using System.Threading;
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
    private readonly DeviceCapsCache _deviceCapsCache;

    private readonly TextBox _displayName = new();
    private readonly ImageView _deviceIcon = new();
    private readonly Label _deviceName = new();
    private readonly Label _deviceDriver = new();
    private readonly LayoutVisibility _deviceVis = new(false);
    private readonly Button _chooseDevice = new() { Text = UiStrings.ChooseDevice };
    private readonly Button _ok = new() { Text = UiStrings.OK };
    private readonly Button _cancel = new() { Text = UiStrings.Cancel };

    private DeviceChoice _currentDevice = DeviceChoice.None;
    private bool _result;
    private CancellationTokenSource? _loadIconCts;

    public SharedDeviceForm(Naps2Config config, IScanPerformer scanPerformer, ErrorOutput errorOutput,
        ISharedDeviceManager sharedDeviceManager, DeviceCapsCache deviceCapsCache) : base(config)
    {
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _sharedDeviceManager = sharedDeviceManager;
        _deviceCapsCache = deviceCapsCache;
        _ok.Click += Ok_Click;
        _cancel.Click += Cancel_Click;

        _chooseDevice.Click += ChooseDevice;
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.SharedDeviceFormTitle;
        Icon = new Icon(1f, Icons.wireless16.ToEtoImage());

        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.DisplayNameLabel),
            _displayName,
            C.Spacer(),
            L.GroupBox(UiStrings.DeviceLabel,
                L.Row(
                    _deviceIcon.Visible(_deviceVis).AlignCenter().NaturalWidth(48),
                    L.Column(
                        C.Filler(),
                        _deviceName,
                        _deviceDriver,
                        C.Filler()
                    ).Spacing(5).Visible(_deviceVis).Scale(),
                    _chooseDevice.AlignCenter()
                )
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

    public DeviceChoice CurrentDevice
    {
        get => _currentDevice;
        set
        {
            _currentDevice = value;
            if (value == DeviceChoice.None)
            {
                _deviceName.Text = "";
                _deviceVis.IsVisible = false;
            }
            else
            {
                _deviceName.Text = value.Device?.Name;
                _deviceDriver.Text = value.Driver switch
                {
                    Driver.Wia => UiStrings.WiaDriver,
                    Driver.Twain => UiStrings.TwainDriver,
                    Driver.Sane => UiStrings.SaneDriver,
                    Driver.Escl => UiStrings.EsclDriver,
                    Driver.Apple => UiStrings.AppleDriver,
                    _ => ""
                };
                _deviceVis.IsVisible = true;
            }
        }
    }

    private int Port { get; set; }

    private int TlsPort { get; set; }

    private Driver DeviceDriver { get; set; } = ScanOptionsValidator.SystemDefaultDriver;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (SharedDevice != null)
        {
            _displayName.Text = SharedDevice.Name;
            CurrentDevice = DeviceChoice.ForDevice(SharedDevice.Device);
            Port = SharedDevice.Port;
            TlsPort = SharedDevice.TlsPort;
            DeviceDriver = SharedDevice.Device.Driver;
        }

        SetDeviceIcon(CurrentDevice.Device?.IconUri);
    }

    private async void ChooseDevice(object? sender, EventArgs args)
    {
        var profile = new ScanProfile { DriverName = DeviceDriver.ToString().ToLowerInvariant() };
        var device = await _scanPerformer.PromptForDevice(profile, false, NativeHandle);
        if (device.Device != null)
        {
            if (string.IsNullOrEmpty(_displayName.Text) ||
                CurrentDevice != DeviceChoice.None && CurrentDevice.Device?.Name == _displayName.Text)
            {
                _displayName.Text = device.Device.Name;
            }
            CurrentDevice = device;
            DeviceDriver = device.Driver;
            SetDeviceIcon(CurrentDevice.Device?.IconUri);
        }
    }

    private void SaveSettings()
    {
        SharedDevice = new SharedDevice
        {
            Name = _displayName.Text,
            Device = CurrentDevice.Device!,
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

    private void SetDeviceIcon(string? iconUri)
    {
        var cachedIcon = _deviceCapsCache.GetCachedIcon(iconUri);
        _deviceIcon.Image =
            cachedIcon ?? (_currentDevice.AlwaysAsk ? Icons.ask.ToEtoImage() : Icons.device.ToEtoImage());
        LayoutController.Invalidate();
        if (cachedIcon == null && iconUri != null)
        {
            ReloadDeviceIcon(iconUri);
        }
    }

    private void ReloadDeviceIcon(string iconUri)
    {
        var cts = new CancellationTokenSource();
        _loadIconCts?.Cancel();
        _loadIconCts = cts;
        Task.Run(async () =>
        {
            var icon = await _deviceCapsCache.LoadIcon(iconUri);
            if (icon != null)
            {
                Invoker.Current.Invoke(() =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        _deviceIcon.Image = icon;
                        LayoutController.Invalidate();
                    }
                });
            }
        });
    }

    private void Ok_Click(object? sender, EventArgs e)
    {
        if (_displayName.Text == "")
        {
            _errorOutput.DisplayError(MiscResources.NameMissing);
            return;
        }
        if (CurrentDevice.Device == null)
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
}