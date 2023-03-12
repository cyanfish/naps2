using System.Threading;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class SelectDeviceForm : EtoDialogBase
{
    private readonly ErrorOutput _errorOutput;
    private readonly ListBox _devices = new();
    private readonly Button _selectDevice;
    private readonly List<ScanDevice> _lazyDeviceList = new();
    // TODO: The spinner doesn't seem to animate on WinForms
    private readonly Spinner _spinner = new() { Enabled = true };

    public SelectDeviceForm(Naps2Config config, ErrorOutput errorOutput) : base(config)
    {
        _errorOutput = errorOutput;
        _selectDevice = C.OkButton(this, SelectDevice, UiStrings.Select);
        _selectDevice.Enabled = false;
    }

    protected override void BuildLayout()
    {
        Title = UiStrings.SelectSource;

        FormStateController.SaveFormState = false;
        FormStateController.RestoreFormState = false;
        FormStateController.DefaultExtraLayoutSize = new Size(50, 0);

        LayoutController.Content = L.Row(
            _devices.NaturalSize(150, 100).Scale(),
            L.Column(
                _selectDevice,
                C.CancelButton(this),
                _spinner.AlignCenter()
            )
        );
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DeviceList != null)
        {
            // If we have a full device list, show it immediately.
            ShowImmediateDeviceList();
        }
        else if (AsyncDevices != null)
        {
            // If we have an IAsyncEnumerable, lazily populate the device list.
            Task.Run(ShowAsyncDeviceList);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    private async Task ShowAsyncDeviceList()
    {
        try
        {
            await foreach (var device in AsyncDevices!)
            {
                Invoker.Current.Invoke(() =>
                {
                    _lazyDeviceList.Add(device);
                    _devices.Items.Add(new ListItem
                    {
                        Key = device.ID,
                        Text = device.Name
                    });
                    if (_devices.Items.Count == 1)
                    {
                        _devices.SelectedIndex = 0;
                        _selectDevice.Enabled = true;
                    }
                });
            }
            Invoker.Current.Invoke(() =>
            {
                _spinner.Visible = false;
                if (_lazyDeviceList.Count == 0 && !AsyncCancelToken.IsCancellationRequested)
                {
                    Close();
                    _errorOutput.DisplayError(SdkResources.NoDevicesFound);
                }
            });
        }
        catch (Exception ex)
        {
            Invoker.Current.Invoke(() =>
            {
                _spinner.Visible = false;
                if (_lazyDeviceList.Count == 0 && !AsyncCancelToken.IsCancellationRequested)
                {
                    Close();
                }
                _errorOutput.DisplayError(ex.Message, ex);
            });
        }
    }

    private void ShowImmediateDeviceList()
    {
        foreach (var device in DeviceList!)
        {
            _devices.Items.Add(new ListItem
            {
                Key = device.ID,
                Text = device.Name
            });
        }
        if (_devices.Items.Count > 0)
        {
            _devices.SelectedIndex = 0;
            _selectDevice.Enabled = true;
        }
        _spinner.Visible = false;
    }

    public IAsyncEnumerable<ScanDevice>? AsyncDevices { get; set; }

    public CancellationToken AsyncCancelToken { get; set; }

    public List<ScanDevice>? DeviceList { get; set; }

    public ScanDevice? SelectedDevice { get; private set; }

    private void SelectDevice()
    {
        if (_devices.SelectedValue == null)
        {
            _devices.Focus();
            return;
        }
        SelectedDevice = (DeviceList ?? _lazyDeviceList).FirstOrDefault(x => x.ID == _devices.SelectedKey);
    }
}