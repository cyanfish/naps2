using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Ui;

namespace NAPS2.EtoForms;

public class FilePathWithPlaceholders
{
    private readonly IFormBase _form;
    private readonly DialogHelper? _dialogHelper;

    private readonly TextBox _path = new();
    private readonly Button _choose = new() { Text = UiStrings.Ellipsis };

    public FilePathWithPlaceholders(IFormBase form, DialogHelper? dialogHelper = null)
    {
        _form = form;
        _dialogHelper = dialogHelper;
        _choose.Click += OpenPathDialog;
        _path.TextChanged += (_, _) => TextChanged?.Invoke(this, EventArgs.Empty);
    }

    public string? Text
    {
        get => _path.Text;
        set => _path.Text = value;
    }

    public event EventHandler? TextChanged;

    public static implicit operator LayoutElement(FilePathWithPlaceholders control)
    {
        return L.Column(
            L.Row(
                control._path.Scale().AlignCenter(),
                control._dialogHelper != null
                    ? control._choose.Width(40).MaxHeight(22)
                    : C.None()
            ).SpacingAfter(2),
            C.Link(UiStrings.Placeholders, control.OpenPlaceholdersForm)
        );
    }

    private void OpenPathDialog(object? sender, EventArgs e)
    {
        if (_dialogHelper.PromptToSavePdf(_path.Text, out string? savePath))
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
}