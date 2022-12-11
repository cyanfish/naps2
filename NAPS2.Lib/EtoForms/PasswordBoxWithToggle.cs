using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public class PasswordBoxWithToggle
{
    private readonly TextBox _plain = new();
    private readonly PasswordBox _hidden = new();
    private readonly CheckBox _show = new() { Text = UiStrings.Show };
    private readonly Panel _panel = new();

    private string? _textCache;
    private bool _suppressEvents;

    public PasswordBoxWithToggle()
    {
        _plain.TextChanged += (_, _) => Text = _plain.Text;
        _hidden.TextChanged += (_, _) => Text = _hidden.Text;
        _show.CheckedChanged += (_, _) =>
        {
            UpdatePanel();
            Focus();
        };
        UpdatePanel();
    }

    private void UpdatePanel()
    {
        _panel.Content = _show.IsChecked() ? _plain : _hidden;
    }

    public string? Title { get; set; }

    public string? Text
    {
        get => _textCache;
        set
        {
            if (value == _textCache || _suppressEvents) return;
            _suppressEvents = true;
            _textCache = value;
            _plain.Text = value;
            _hidden.Text = value;
            _suppressEvents = false;
            TextChanged?.Invoke();
        }
    }

    public bool Enabled
    {
        get => _plain.Enabled;
        set
        {
            _plain.Enabled = value;
            _hidden.Enabled = value;
            _show.Enabled = value;
        }
    }

    public int TitleWrapWidth { get; set; }

    public static implicit operator LayoutElement(PasswordBoxWithToggle control)
    {
        return L.Column(
            control.Inline()
        );
    }

    public LayoutElement Inline()
    {
        return new LayoutElement[]
        {
            L.Row(
                GetTitleElement(),
                _show.Padding(left: 10)
            ).Aligned().SpacingAfter(2),
            _panel.Height(20)
        }.Expand();
    }

    private LayoutElement GetTitleElement()
    {
        if (string.IsNullOrEmpty(Title))
        {
            return C.Filler();
        }
        var label = C.Label(Title);
        return TitleWrapWidth > 0 ? label.Wrap(TitleWrapWidth).Scale() : label.Scale();
    }

    public event Action? TextChanged;

    public void Focus()
    {
        if (_show.IsChecked())
        {
            _plain.Focus();
        }
        else
        {
            _hidden.Focus();
        }
    }
}