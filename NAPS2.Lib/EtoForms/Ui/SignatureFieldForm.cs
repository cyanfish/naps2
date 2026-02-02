using Eto.Drawing;
using Eto.Forms;
using NAPS2.Pdf;

namespace NAPS2.EtoForms.Ui;

public class SignatureFieldForm : UnaryImageFormBase
{
    private const int BORDER_WIDTH = 2;
    private const int MIN_FIELD_SIZE = 20;

    private readonly ColorScheme _colorScheme;

    // Mouse down location
    private PointF _mouseOrigin;

    // Field placement as fractions of the total image size (updated as the user drags)
    private float _fieldX, _fieldY, _fieldW, _fieldH;

    // Field placement as pixels (updated on mouse up)
    private float _realX, _realY, _realW, _realH;

    private bool _isDragging;
    private bool _hasPlacement;

    public SignatureFieldForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        ColorScheme colorScheme, IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Title = "Place Signature Field";
        IconName = "document_sign_small";

        _colorScheme = colorScheme;

        OverlayBorderSize = BORDER_WIDTH;
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        DefaultButton.Focus();
    }

    protected override void Apply()
    {
        if (!_hasPlacement)
        {
            // No field placed, nothing to do
            return;
        }

        // Create signature field placement
        var fieldPlacement = SignatureFieldPlacement.FromPixels(
            $"Signature_{Guid.NewGuid():N}",
            _realX,
            _realY,
            _realW,
            _realH,
            RealImageWidth,
            RealImageHeight);

        // Get the current processed image
        using var processedImage = Image.GetClonedImage();
        
        // Update the post-processing data with the signature field
        var currentFields = processedImage.PostProcessingData.SignatureFields ?? new List<SignatureFieldPlacement>();
        var updatedFields = new List<SignatureFieldPlacement>(currentFields) { fieldPlacement };

        var updatedPostProcessingData = processedImage.PostProcessingData with
        {
            SignatureFields = updatedFields
        };

        // Create updated processed image
        var updatedProcessedImage = processedImage.WithPostProcessingData(updatedPostProcessingData, false);
        
        // Replace the internal image in the UiImage
        Image.ReplaceInternalImage(updatedProcessedImage);
    }

    protected override void Revert()
    {
        _fieldX = _fieldY = _fieldW = _fieldH = 0;
        _realX = _realY = _realW = _realH = 0;
        _hasPlacement = false;
        Overlay.Invalidate();
    }

    protected override IMemoryImage RenderPreview()
    {
        return WorkingImage!.Clone();
    }

    protected override List<Transform> Transforms => new List<Transform>();

    private void Overlay_MouseDown(object? sender, MouseEventArgs e)
    {
        _isDragging = true;
        _mouseOrigin = e.Location;
        Overlay.Invalidate();
    }

    private void Overlay_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_isDragging && _fieldW > 0 && _fieldH > 0)
        {
            _realX = _fieldX * RealImageWidth;
            _realY = _fieldY * RealImageHeight;
            _realW = _fieldW * RealImageWidth;
            _realH = _fieldH * RealImageHeight;
            _hasPlacement = true;
        }
        _isDragging = false;
        Overlay.Invalidate();
    }

    private void UpdateFieldPlacement(PointF mousePos)
    {
        if (!_isDragging) return;

        var delta = mousePos - _mouseOrigin;
        
        // Convert to overlay-relative coordinates
        var origin = _mouseOrigin - new PointF(_overlayL, _overlayT);
        var current = mousePos - new PointF(_overlayL, _overlayT);

        // Calculate normalized coordinates (handle negative deltas)
        if (delta.Y > 0)
        {
            _fieldY = (origin.Y / _overlayH).Clamp(0, 1);
            _fieldH = ((current.Y - origin.Y) / _overlayH).Clamp(0, 1 - _fieldY);
        }
        else
        {
            _fieldY = (current.Y / _overlayH).Clamp(0, 1);
            _fieldH = ((origin.Y - current.Y) / _overlayH).Clamp(0, 1 - _fieldY);
        }

        if (delta.X > 0)
        {
            _fieldX = (origin.X / _overlayW).Clamp(0, 1);
            _fieldW = ((current.X - origin.X) / _overlayW).Clamp(0, 1 - _fieldX);
        }
        else
        {
            _fieldX = (current.X / _overlayW).Clamp(0, 1);
            _fieldW = ((origin.X - current.X) / _overlayW).Clamp(0, 1 - _fieldX);
        }
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        Overlay.Cursor = Cursors.Crosshair;
        UpdateFieldPlacement(e.Location);
        Overlay.Invalidate();
    }

    protected override void PaintOverlay(object? sender, PaintEventArgs e)
    {
        base.PaintOverlay(sender, e);

        if (_overlayW == 0 || _overlayH == 0)
        {
            return;
        }

        // Draw existing signature fields from post-processing data
        using var processedImage = Image.GetClonedImage();
        if (processedImage.PostProcessingData.SignatureFields != null)
        {
            var existingFieldPen = new Pen(Color.FromArgb(100, 0, 200, 0), BORDER_WIDTH);
            foreach (var field in processedImage.PostProcessingData.SignatureFields)
            {
                var (x, y, w, h) = field.ToPixels(RealImageWidth, RealImageHeight);
                var overlayX = _overlayL + (x / RealImageWidth) * _overlayW;
                var overlayY = _overlayT + (y / RealImageHeight) * _overlayH;
                var overlayW = (w / RealImageWidth) * _overlayW;
                var overlayH = (h / RealImageHeight) * _overlayH;
                
                e.Graphics.DrawRectangle(existingFieldPen, overlayX, overlayY, overlayW, overlayH);
            }
        }

        // Draw the current field being placed
        if (_isDragging && (_fieldW > 0 || _fieldH > 0))
        {
            var offsetX = _fieldX * _overlayW;
            var offsetY = _fieldY * _overlayH;
            var offsetW = _fieldW * _overlayW;
            var offsetH = _fieldH * _overlayH;

            var x = _overlayL + offsetX;
            var y = _overlayT + offsetY;

            // Draw border
            var fieldPen = new Pen(_colorScheme.CropColor, BORDER_WIDTH);
            e.Graphics.DrawRectangle(fieldPen, x, y, offsetW, offsetH);

            // Draw semi-transparent fill
            var fillColor = new Color(_colorScheme.CropColor, 0.2f);
            e.Graphics.FillRectangle(fillColor, x, y, offsetW, offsetH);
        }
    }
}
