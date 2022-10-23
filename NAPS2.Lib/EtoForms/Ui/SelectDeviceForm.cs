using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Scan;

namespace NAPS2.EtoForms.Ui;

public class SelectDeviceForm : EtoDialogBase
{
    private readonly ListBox _devices;

    public SelectDeviceForm(Naps2Config config) : base(config)
    {
        Resizable = true;
        FormStateController.SaveFormState = false;
        FormStateController.RestoreFormState = false;

        var selectButton = new Button
        {
            Text = UiStrings.Select
        };
        DefaultButton = selectButton;
        var cancelButton = new Button
        {
            Text = UiStrings.Cancel
        };
        AbortButton = cancelButton;

        _devices = new ListBox();

        selectButton.Click += Select_Click;
        cancelButton.Click += Cancel_Click;

        Title = UiStrings.SelectSource;
        FormStateController.DefaultExtraLayoutSize = new Size(50, 0);
        LayoutController.Content = L.Row(
            _devices.NaturalSize(150, 100).XScale(),
            L.Column(
                selectButton,
                cancelButton,
                C.ZeroSpace().YScale()
            )
        );
    }

    public List<ScanDevice> DeviceList { get; set; } = null!;

    public ScanDevice? SelectedDevice { get; private set; }

    protected override void OnPreLoad(EventArgs e)
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
        // TODO: Re-order things so we don't need to do this twice
        // Just need to be careful not to break things cross-platform
        FormStateController.UpdateLayoutSize(LayoutController);
        base.OnPreLoad(e);
    }

    private void Select_Click(object? sender, EventArgs e)
    {
        if (_devices.SelectedValue == null)
        {
            _devices.Focus();
            return;
        }
        SelectedDevice = DeviceList.FirstOrDefault(x => x.ID == _devices.SelectedKey);
        Close();
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        Close();
    }
}