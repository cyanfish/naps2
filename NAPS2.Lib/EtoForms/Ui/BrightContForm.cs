using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class BrightContForm : ImageFormBase
{
    private readonly Slider _brightnessSlider = new() { MinValue = -1000, MaxValue = 1000, TickFrequency = 200 };
    private readonly NumericMaskedTextBox<int> _brightnessText = new() { Text = "0" };
    private readonly Slider _contrastSlider = new() { MinValue = -1000, MaxValue = 1000, TickFrequency = 200 };
    private readonly NumericMaskedTextBox<int> _contrastText = new() { Text = "0" };

    public BrightContForm(Naps2Config config, ThumbnailController thumbnailController) : base(config,
        thumbnailController)
    {
        // TODO: Create a helper for this
        _contrastSlider.ValueChanged += ContrastSlider_Scroll;
        _contrastText.TextChanged += ContrastText_TextChanged;
        _brightnessSlider.ValueChanged += BrightnessSlider_Scroll;
        _brightnessText.TextChanged += BrightnessText_TextChanged;
    }

    private void BrightnessText_TextChanged(object? sender, EventArgs e)
    {
        if (int.TryParse(_brightnessText.Text, out int value))
        {
            if (value >= _brightnessSlider.MinValue && value <= _brightnessSlider.MaxValue)
            {
                _brightnessSlider.Value = value;
            }
        }
        UpdateTransform();
    }

    private void BrightnessSlider_Scroll(object? sender, EventArgs e)
    {
        _brightnessText.Text = _brightnessSlider.Value.ToString("G");
        UpdateTransform();
    }

    private void ContrastText_TextChanged(object? sender, EventArgs e)
    {
        if (int.TryParse(_contrastText.Text, out int value))
        {
            if (value >= _contrastSlider.MinValue && value <= _contrastSlider.MaxValue)
            {
                _contrastSlider.Value = value;
            }
        }
        UpdateTransform();
    }

    private void ContrastSlider_Scroll(object? sender, EventArgs e)
    {
        _contrastText.Text = _contrastSlider.Value.ToString("G");
        UpdateTransform();
    }

    public BrightnessTransform BrightnessTransform { get; private set; } = new BrightnessTransform();

    public TrueContrastTransform TrueContrastTransform { get; private set; } = new TrueContrastTransform();

    protected override LayoutElement CreateControls()
    {
        return L.Column(
            L.Row(_brightnessSlider.XScale(), _brightnessText.AlignCenter()),
            L.Row(_contrastSlider.XScale(), _contrastText.AlignCenter())
        );
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[] { BrightnessTransform, TrueContrastTransform };

    protected override void ResetTransform()
    {
        BrightnessTransform = new BrightnessTransform();
        TrueContrastTransform = new TrueContrastTransform();
        _brightnessSlider.Value = 0;
        _contrastSlider.Value = 0;
    }

    private void UpdateTransform()
    {
        BrightnessTransform = new BrightnessTransform(_brightnessSlider.Value);
        TrueContrastTransform = new TrueContrastTransform(_contrastSlider.Value);
        UpdatePreviewBox();
    }
}