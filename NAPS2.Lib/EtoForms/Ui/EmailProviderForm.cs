using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.ImportExport.Email;

namespace NAPS2.EtoForms.Ui;

internal class EmailProviderForm : EtoDialogBase
{
    private readonly EmailProviderController _controller;

    public EmailProviderForm(Naps2Config config, EmailProviderController controller, IIconProvider iconProvider) :
        base(config)
    {
        Title = UiStrings.EmailProviderFormTitle;
        Icon = new Icon(1f, iconProvider.GetIcon("email_small"));

        _controller = controller;
    }

    protected override void BuildLayout()
    {
        FormStateController.FixedHeightLayout = true;

        LayoutController.DefaultSpacing = 0;
        LayoutController.Content = L.Column(
            _controller.GetWidgets().Select(x => C.Button(new ActionCommand(() =>
            {
                if (x.Choose())
                {
                    Result = true;
                    Close();
                }
            })
            {
                Text = x.ProviderName,
                Image = x.ProviderIcon,
                Enabled = x.Enabled
            }, ButtonImagePosition.Left, big: true).NaturalWidth(500).Height(50)).Expand()
        );
    }

    public bool Result { get; private set; }
}