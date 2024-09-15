using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Widgets;

public class SliderWithTextBox
{
    public static readonly Constraints DefaultConstraints = new IntConstraints(-1000, 1000, 200);

    private readonly Constraints _constraints;
    private readonly Slider _slider = new();
    private readonly ImageView _imageView = new();
    private readonly LayoutVisibility _imageVis = new(false);
    private readonly TextBox _textBox;

    private int _valueCache;

    public SliderWithTextBox() : this(DefaultConstraints)
    {
    }

    public SliderWithTextBox(Constraints constraints)
    {
        _constraints = constraints;
        _textBox = constraints.IsInteger
            ? new NumericMaskedTextBox<int>()
            : new NumericMaskedTextBox<double>();
        _slider.MinValue = constraints.SliderMinValue;
        _slider.MaxValue = constraints.SliderMaxValue;
        _slider.TickFrequency = constraints.SliderTickFrequency;
        _textBox.Text = 0.ToString("G");

        _slider.ValueChanged += (_, _) => { IntValue = _slider.Value; };
        _textBox.TextChanged += (_, _) =>
        {
            if (double.TryParse(_textBox.Text, out double value))
            {
                if (!constraints.IsInteger)
                {
                    value *= constraints.Multiplier;
                }
                value = Math.Round(value);
                if (value >= constraints.SliderMinValue && value <= constraints.SliderMaxValue)
                {
                    IntValue = (int) value;
                }
            }
        };
    }

    public int IntValue
    {
        get => _valueCache;
        set
        {
            if (value == _valueCache) return;
            _valueCache = value;
            _slider.Value = value;
            _textBox.Text = _constraints.IsInteger
                ? value.ToString("G")
                : (value / (decimal) _constraints.Multiplier).ToString("G");
            ValueChanged?.Invoke();
        }
    }

    public decimal DecimalValue
    {
        get => IntValue / (decimal) _constraints.Multiplier;
        set => IntValue = (int) Math.Round(value * _constraints.Multiplier);
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

    public Image? Icon
    {
        get => _imageView.Image;
        set
        {
            _imageView.Image = value;
            _imageVis.IsVisible = value != null;
        }
    }

    public static implicit operator LayoutElement(SliderWithTextBox control)
    {
        return control.AsControl();
    }

    public event Action? ValueChanged;

    public LayoutRow AsControl()
    {
        return L.Row(
            _imageView
                .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Leading : LayoutAlignment.Center)
                .Padding(top: 2, bottom: 2).Visible(_imageVis),
            _slider.Scale(),
            _textBox.Width(EtoPlatform.Current.IsGtk ? 50 : 40)
                .Align(EtoPlatform.Current.IsWinForms ? LayoutAlignment.Leading : LayoutAlignment.Center)
        );
    }

    public abstract class Constraints
    {
        public bool IsInteger { get; protected init; }
        public int Multiplier { get; protected init; }
        public int SliderMinValue { get; protected init; }
        public int SliderMaxValue { get; protected init; }
        public int SliderTickFrequency { get; protected init; }
    }

    public class IntConstraints : Constraints
    {
        public IntConstraints(int minValue, int maxValue, int tickFrequency)
        {
            IsInteger = true;
            Multiplier = 1;
            SliderMinValue = minValue;
            SliderMaxValue = maxValue;
            SliderTickFrequency = tickFrequency;
        }
    }

    public class DecimalConstraints : Constraints
    {
        public DecimalConstraints(decimal minValue, decimal maxValue, decimal tickFrequency, int decimalPlaces)
        {
            IsInteger = false;
            Multiplier = (int) Math.Pow(10, decimalPlaces);
            SliderMinValue = (int) (minValue * Multiplier);
            SliderMaxValue = (int) (maxValue * Multiplier);
            SliderTickFrequency = (int) (tickFrequency * Multiplier);
        }
    }
}