using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

internal class EditWithForm : EtoDialogBase
{
    private readonly IOsOpenWith _openWith;

    public EditWithForm(Naps2Config config, IOsOpenWith openWith) :
        base(config)
    {
        Title = UiStrings.EditWithFormTitle;
        IconName = "pencil_small";

        _openWith = openWith;
    }

    protected override void BuildLayout()
    {
        FormStateController.FixedHeightLayout = true;

        LayoutController.DefaultSpacing = 0;
        LayoutController.Content = L.Column(
            _openWith.GetEntries(".jpg").Select(entry => C.Button(new ActionCommand(() =>
                {
                    Result = entry;
                    Close();
                })
                {
                    Text = entry.Name,
                    Image = _openWith.LoadIcon(entry)?.ToEtoImage(),
                    // TODO: Provide a default icon if it couldn't be loaded?
                    // IconName = ??
                }, ButtonImagePosition.Left, ButtonFlags.LargeText | ButtonFlags.LargeIcon).NaturalWidth(500)
                .Height(50))
                .Expand()
        );
    }

    public OpenWithEntry? Result { get; private set; }
}