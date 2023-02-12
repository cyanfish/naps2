using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class SelectDeviceForm : EtoDialogBase
{
    private readonly ListBox _devices = new();
    private readonly Button _selectDevice;
    private readonly List<ScanDevice> _lazyDeviceList = new();
    // TODO: The spinner doesn't seem to animate on WinForms
    private readonly Spinner _spinner = new() { Enabled = true };
    // TODO: As we don't care to relayout, we should just be able to set .Visible=false without it resetting on form resize
    private readonly LayoutVisibility _spinnerVisible = new(true);

    public SelectDeviceForm(Naps2Config config) : base(config)
    {
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
                _spinner.AlignCenter().Visible(_spinnerVisible)
            )
        );
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DeviceList != null)
        {
            // If we have a full device list, show it immediately.
            foreach (var device in DeviceList)
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
            _spinnerVisible.IsVisible = false;
        }
        else if (AsyncDevices != null)
        {
            // If we have an IAsyncEnumerable, lazily populate the device list.
            Task.Run(async () =>
            {
                await foreach (var device in AsyncDevices)
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
                Invoker.Current.Invoke(() => _spinnerVisible.IsVisible = false);
            });
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public IAsyncEnumerable<ScanDevice>? AsyncDevices { get; set; }

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