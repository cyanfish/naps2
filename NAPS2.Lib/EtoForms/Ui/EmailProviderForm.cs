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
        IconName = "email_small";

        _controller = controller;
    }

    protected override void BuildLayout()
    {
        FormStateController.FixedHeightLayout = true;
        base.BuildLayout();

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
                    IconName = x.ProviderIconName,
                    Enabled = x.Enabled
                }, ButtonImagePosition.Left, ButtonFlags.LargeText | ButtonFlags.LargeIcon).NaturalWidth(500)
                .Height(50))
                .Expand()
        );
    }

    public bool Result { get; private set; }
}