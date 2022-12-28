using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms.Widgets;

public class FilePathWithPlaceholders
{
    private readonly IFormBase _form;
    private readonly DialogHelper? _dialogHelper;

    private readonly TextBox _path = new();
    private readonly Button _choose = new() { Text = UiStrings.Ellipsis };
    private readonly LinkButton _placeholders = C.Link(UiStrings.Placeholders);
    private LayoutVisibility? _visibility;

    public FilePathWithPlaceholders(IFormBase form, DialogHelper? dialogHelper = null)
    {
        _form = form;
        _dialogHelper = dialogHelper;
        _choose.Click += OpenPathDialog;
        _path.TextChanged += (_, _) => TextChanged?.Invoke(this, EventArgs.Empty);
        _placeholders.Click += (_, _) => OpenPlaceholdersForm();
    }

    public string? Text
    {
        get => _path.Text;
        set => _path.Text = value;
    }

    public bool Enabled
    {
        get => _path.Enabled;
        set
        {
            _path.Enabled = value;
            _choose.Enabled = value;
            _placeholders.Enabled = value;
        }
    }

    public event EventHandler? TextChanged;

    public static implicit operator LayoutElement(FilePathWithPlaceholders control)
    {
        return control.AsControl();
    }

    public LayoutColumn AsControl()
    {
        return L.Column(
            L.Row(
                _path.Scale().AlignCenter().Visible(_visibility),
                _dialogHelper != null
                    ? _choose.Width(EtoPlatform.Current.IsGtk ? null : 40).MaxHeight(22).Visible(_visibility)
                    : C.None()
            ).SpacingAfter(2),
            _placeholders.Visible(_visibility)
        );
    }

    private void OpenPathDialog(object? sender, EventArgs e)
    {
        if (_dialogHelper!.PromptToSavePdf(_path.Text, out string? savePath))
        {
            _path.Text = savePath!;
        }
    }

    private void OpenPlaceholdersForm()
    {
        var form = _form.FormFactory.Create<PlaceholdersForm>();
        form.FileName = _path.Text;
        form.ShowModal();
        if (form.Updated)
        {
            _path.Text = form.FileName;
        }
    }

    public void Focus()
    {
        _path.Focus();
    }

    // TODO: Better solution
    public FilePathWithPlaceholders Visible(LayoutVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }
}