using System.Globalization;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class ResolutionForm : EtoDialogBase
{
    private readonly NumericMaskedTextBox<int> _dpiTextbox = new();

    public ResolutionForm(Naps2Config config)
        : base(config)
    {
    }

    public int Dpi { get; set; }

    public bool Result { get; private set; }

    protected override void BuildLayout()
    {
        _dpiTextbox.Text = Dpi == 0 ? "" : Dpi.ToString(CultureInfo.InvariantCulture);
        _dpiTextbox.SelectAll();
        _dpiTextbox.Focus();

        Title = UiStrings.ResolutionFormTitle;

        FormStateController.RestoreFormState = false;
        FormStateController.FixedHeightLayout = true;

        LayoutController.Content = L.Column(
            C.Label(UiStrings.Dpi),
            _dpiTextbox,
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Submit),
                    C.CancelButton(this))
            )
        );
    }


    private void Submit()
    {
        const NumberStyles numberStyle = NumberStyles.AllowLeadingWhite;
        if (!int.TryParse(_dpiTextbox.Text, numberStyle, CultureInfo.CurrentCulture, out int dpi) || dpi <= 0)
        {
            _dpiTextbox.Focus();
            return;
        }
        Dpi = dpi;
        Result = true;
    }
}