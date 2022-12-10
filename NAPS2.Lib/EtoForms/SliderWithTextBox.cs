using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public class SliderWithTextBox
{
    private readonly Slider _slider = new() { MinValue = -1000, MaxValue = 1000, TickFrequency = 200 };
    private readonly NumericMaskedTextBox<int> _textBox = new() { Text = 0.ToString("G") };

    private int _valueCache;

    public SliderWithTextBox()
    {
        _slider.ValueChanged += (_, _) => { Value = _slider.Value; };
        _textBox.TextChanged += (_, _) =>
        {
            if (int.TryParse(_textBox.Text, out int value))
            {
                if (value >= MinValue && value <= MaxValue)
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
        get => _valueCache;
        set
        {
            if (value == _valueCache) return;
            _valueCache = value;
            _slider.Value = value;
            _textBox.Text = value.ToString("G");
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

    public Image? Icon { get; set; }

    public static implicit operator LayoutElement(SliderWithTextBox control)
    {
        return control.AsControl();
    }

    public event Action? ValueChanged;

    public LayoutRow AsControl()
    {
        return L.Row(
            Icon != null
                ? new ImageView { Image = Icon }
                    .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Leading : LayoutAlignment.Center)
                    .Padding(top: 2, bottom: 2)
                : C.None(),
            _slider.Scale(),
            _textBox.Width(EtoPlatform.Current.IsGtk ? 50 : 40)
                .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Leading : LayoutAlignment.Center)
        );
    }
}