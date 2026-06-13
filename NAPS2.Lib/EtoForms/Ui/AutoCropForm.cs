using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using NAPS2.Config;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Layout;
using NAPS2.EtoForms.Widgets;
using NAPS2.Images;

namespace NAPS2.EtoForms.Ui;

public class AutoCropForm : EtoDialogBase
{
    private readonly ImageListActions _imageListActions;

    private readonly EnumDropDownWidget<AutoCropAxisMode> _widthMode = new();
    private readonly EnumDropDownWidget<AutoCropAxisMode> _heightMode = new();
    private readonly TextBox _fixedWidth = new();
    private readonly TextBox _fixedHeight = new();
    private readonly TextBox _padding = new();

    public AutoCropForm(Naps2Config config, ImageListActions imageListActions) : base(config)
    {
        _imageListActions = imageListActions;
        Title = UiStrings.AutoCropFormTitle;

        _widthMode.Format = AxisModeText;
        _heightMode.Format = AxisModeText;
        _widthMode.SelectedItemChanged += (_, _) => UpdateEnabled();
        _heightMode.SelectedItemChanged += (_, _) => UpdateEnabled();
    }

    private static string AxisModeText(AutoCropAxisMode mode) => mode switch
    {
        AutoCropAxisMode.Off => UiStrings.AutoCropModeOff,
        AutoCropAxisMode.Auto => UiStrings.AutoCropModeAuto,
        AutoCropAxisMode.Fixed => UiStrings.AutoCropModeFixed,
        _ => mode.ToString()
    };

    protected override void BuildLayout()
    {
        var cfg = Config.Get(c => c.AutoCrop);
        _widthMode.SelectedItem = cfg.WidthMode;
        _heightMode.SelectedItem = cfg.HeightMode;
        _fixedWidth.Text = MmText(cfg.FixedWidthMm);
        _fixedHeight.Text = MmText(cfg.FixedHeightMm);
        _padding.Text = MmText(cfg.PaddingMm);
        UpdateEnabled();

        FormStateController.FixedHeightLayout = true;
        FormStateController.DefaultExtraLayoutSize = new Size(60, 0);

        LayoutController.Content = L.Column(
            C.Label(UiStrings.AutoCropHelp).DynamicWrap(360).NaturalWidth(360),
            C.Spacer(),
            L.GroupBox(UiStrings.AutoCropWidthLabel,
                L.Column(
                    _widthMode,
                    L.Row(
                        C.Label(UiStrings.AutoCropFixedSizeLabel).AlignCenter(),
                        _fixedWidth.Width(80)
                    )
                )
            ),
            L.GroupBox(UiStrings.AutoCropHeightLabel,
                L.Column(
                    _heightMode,
                    L.Row(
                        C.Label(UiStrings.AutoCropFixedSizeLabel).AlignCenter(),
                        _fixedHeight.Width(80)
                    )
                )
            ),
            L.Row(
                C.Label(UiStrings.AutoCropPaddingLabel).AlignCenter(),
                _padding.Width(80)
            ),
            C.Filler(),
            L.Row(
                C.Filler(),
                L.OkCancel(
                    C.OkButton(this, Save),
                    C.CancelButton(this))
            )
        );
    }

    private void UpdateEnabled()
    {
        _fixedWidth.Enabled = _widthMode.SelectedItem == AutoCropAxisMode.Fixed;
        _fixedHeight.Enabled = _heightMode.SelectedItem == AutoCropAxisMode.Fixed;
    }

    private static string MmText(double? mm) =>
        mm.HasValue ? mm.Value.ToString(CultureInfo.CurrentCulture) : "";

    private static double? ParseMm(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }
        return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value) && value > 0
            ? value
            : null;
    }

    private bool Save()
    {
        var widthMode = _widthMode.SelectedItem;
        var heightMode = _heightMode.SelectedItem;
        var fixedWidthMm = ParseMm(_fixedWidth.Text);
        var fixedHeightMm = ParseMm(_fixedHeight.Text);

        if (widthMode == AutoCropAxisMode.Fixed && fixedWidthMm == null)
        {
            _fixedWidth.Focus();
            return false;
        }
        if (heightMode == AutoCropAxisMode.Fixed && fixedHeightMm == null)
        {
            _fixedHeight.Focus();
            return false;
        }

        Config.User.Set(c => c.AutoCrop, new AutoCropConfig
        {
            WidthMode = widthMode,
            HeightMode = heightMode,
            FixedWidthMm = fixedWidthMm,
            FixedHeightMm = fixedHeightMm,
            PaddingMm = ParseMm(_padding.Text) ?? 0
        });

        _imageListActions.AutoCrop();
        return true;
    }
}
