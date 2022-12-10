using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class SelectDeviceForm : EtoDialogBase
{
    private readonly ListBox _devices = new();

    public SelectDeviceForm(Naps2Config config) : base(config)
    {
    }

    protected override void BuildLayout()
    {
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
        }

        Title = UiStrings.SelectSource;

        FormStateController.SaveFormState = false;
        FormStateController.RestoreFormState = false;
        FormStateController.DefaultExtraLayoutSize = new Size(50, 0);

        LayoutController.Content = L.Row(
            _devices.NaturalSize(150, 100).XScale(),
            L.Column(
                C.OkButton(this, SelectDevice, UiStrings.Select),
                C.CancelButton(this)
            )
        );
    }

    public List<ScanDevice> DeviceList { get; set; } = null!;

    public ScanDevice? SelectedDevice { get; private set; }

    private void SelectDevice()
    {
        if (_devices.SelectedValue == null)
        {
            _devices.Focus();
            return;
        }
        SelectedDevice = DeviceList.FirstOrDefault(x => x.ID == _devices.SelectedKey);
    }
}