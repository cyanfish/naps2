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
    private readonly Label _missingExtension = new() { Text = UiStrings.MissingFileExtension };
    private readonly LayoutVisibility _missingExtensionVis = new(false);
    private LayoutVisibility? _visibility;

    public FilePathWithPlaceholders(IFormBase form, DialogHelper? dialogHelper = null)
    {
        _form = form;
        _dialogHelper = dialogHelper;
        _choose.Click += OpenPathDialog;
        _path.TextChanged += (_, _) =>
        {
            _missingExtensionVis.IsVisible = IsMissingExtension;
            TextChanged?.Invoke(this, EventArgs.Empty);
        };
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

    public bool PdfOnly { get; set; }

    public bool ImagesOnly { get; set; }

    /// <summary>
    /// If true, a warning is shown when the path doesn't have a file extension that determines the file type (PDF or
    /// image). Use IsMissingExtension to block saving settings in that case.
    /// </summary>
    public bool RequireExtension
    {
        get;
        set
        {
            field = value;
            _missingExtensionVis.IsVisible = IsMissingExtension;
        }
    }

    public bool IsMissingExtension
    {
        get
        {
            if (!RequireExtension || string.IsNullOrWhiteSpace(_path.Text))
            {
                return false;
            }
            return Path.GetExtension(_path.Text).ToLowerInvariant() != ".pdf" &&
                   ImageContext.GetFileFormatFromExtension(_path.Text) == ImageFileFormat.Unknown;
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
                    ? _choose.Width(EtoPlatform.Current.IsGtk ? null : 40)
                        .MaxHeight(EtoPlatform.Current.IsWpf ? null : 22).Visible(_visibility)
                    : C.None()
            ).SpacingAfter(2),
            L.Row(
                _placeholders.Visible(_visibility),
                C.Filler(),
                _missingExtension.Visible(LayoutVisibility.Combine(_visibility, _missingExtensionVis))
            )
        );
    }

    private void OpenPathDialog(object? sender, EventArgs e)
    {
        if (PdfOnly)
        {
            if (_dialogHelper!.PromptToSavePdf(_path.Text, out string? savePath))
            {
                _path.Text = savePath!;
            }
        }
        else if (ImagesOnly)
        {
            if (_dialogHelper!.PromptToSaveImage(_path.Text, out string? savePath))
            {
                _path.Text = savePath!;
            }
        }
        else
        {
            if (_dialogHelper!.PromptToSavePdfOrImage(_path.Text, out string? savePath))
            {
                _path.Text = savePath!;
            }
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