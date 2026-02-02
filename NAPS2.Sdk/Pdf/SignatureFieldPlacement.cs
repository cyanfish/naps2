namespace NAPS2.Pdf;

/// <summary>
/// Represents a signature field placement on a PDF page.
/// Coordinates are stored as normalized fractions (0.0 to 1.0) relative to page dimensions.
/// </summary>
public record SignatureFieldPlacement(
    string FieldName,
    float NormalizedX,
    float NormalizedY,
    float NormalizedWidth,
    float NormalizedHeight)
{
    /// <summary>
    /// Creates a signature field placement from pixel coordinates on a page.
    /// </summary>
    public static SignatureFieldPlacement FromPixels(
        string fieldName,
        float pixelX,
        float pixelY,
        float pixelWidth,
        float pixelHeight,
        float pageWidth,
        float pageHeight)
    {
        return new SignatureFieldPlacement(
            fieldName,
            pixelX / pageWidth,
            pixelY / pageHeight,
            pixelWidth / pageWidth,
            pixelHeight / pageHeight);
    }

    /// <summary>
    /// Converts normalized coordinates to pixel coordinates for a given page size.
    /// </summary>
    public (float x, float y, float width, float height) ToPixels(float pageWidth, float pageHeight)
    {
        return (
            NormalizedX * pageWidth,
            NormalizedY * pageHeight,
            NormalizedWidth * pageWidth,
            NormalizedHeight * pageHeight);
    }
}
