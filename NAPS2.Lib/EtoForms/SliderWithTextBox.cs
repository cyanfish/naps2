using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public class SliderWithTextBox
{
    private readonly Slider _slider = new() { MinValue = -1000, MaxValue = 1000, TickFrequency = 200 };
    private readonly NumericMaskedTextBox<int> _textBox = new() { Text = 0.ToString("G") };

    public SliderWithTextBox()
    {
        _slider.ValueChanged += (_, _) =>
        {
            Value = _slider.Value;
        };
        _textBox.TextChanged += (_, _) =>
        {
            if (int.TryParse(_textBox.Text, out int value))
            {
                if (value != Value && value >= MinValue && value <= MaxValue)
                {
                    Value = value;
                }
            }
        };
    }

    public int MinValue
    {
        get => _slider.MinValue;
        set => _slider.MinValue = value;
    }

    public int MaxValue
    {
        get => _slider.MaxValue;
        set => _slider.MaxValue = value;
    }

    public int TickFrequency
    {
        get => _slider.TickFrequency;
        set => _slider.TickFrequency = value;
    }

    public int Value
    {
        get => _slider.Value;
        set
        {
            if (_slider.Value != value)
            {
                _slider.Value = value;
            }
            var text = value.ToString("G");
            if (_textBox.Text != text)
            {
                _textBox.Text = text;
            }
            ValueChanged?.Invoke();
        }
    }

    public bool Enabled
    {
        get => _slider.Enabled;
        set
        {
            _slider.Enabled = value;
            _textBox.Enabled = value;
        }
    }

    public static implicit operator LayoutElement(SliderWithTextBox control)
    {
        return L.Row(
            control._slider.XScale(),
            control._textBox.Width(EtoPlatform.Current.IsGtk ? 50 : 40)
                .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Leading : LayoutAlignment.Center)
        );
    }

    public event Action? ValueChanged;
}