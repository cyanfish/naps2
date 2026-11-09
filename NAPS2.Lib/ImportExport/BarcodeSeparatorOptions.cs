using NAPS2.Images;
using ZXing;

namespace NAPS2.ImportExport;

/// <summary>
/// Defines which decoded barcodes separate documents and whether the separator page is retained.
/// </summary>
public class BarcodeSeparatorOptions
{
    public List<PatchCodeType> PatchCodes { get; set; } = [];

    public BarcodeFormat? BarcodeType { get; set; }

    public List<BarcodeFormat> BarcodeTypes { get; set; } = [];

    public int MinimumLength { get; set; } = 1;

    public int MaximumLength { get; set; } = int.MaxValue;

    public string Prefix { get; set; } = "";

    public bool RemoveBarcodePage { get; set; }

    internal void Validate()
    {
        if (MinimumLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumLength));
        }
        if (MaximumLength < MinimumLength)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumLength));
        }
        if (Prefix == null)
        {
            throw new ArgumentNullException(nameof(Prefix));
        }
        if (PatchCodes == null)
        {
            throw new ArgumentNullException(nameof(PatchCodes));
        }
        if (BarcodeTypes == null)
        {
            throw new ArgumentNullException(nameof(BarcodeTypes));
        }
    }

    internal bool Matches(Barcode barcode)
    {
        var text = barcode.DetectedText;
        bool hasTypeFilters = PatchCodes.Count > 0 || BarcodeType != null || BarcodeTypes.Count > 0;
        bool matchesType = !hasTypeFilters ||
                           (barcode.PatchCode is { } patchCode && PatchCodes.Contains(patchCode)) ||
                           (barcode.DetectedFormat is { } format &&
                            (BarcodeType == format || BarcodeTypes.Contains(format)));
        return barcode.IsDetected &&
               matchesType &&
               text != null &&
               text.Length >= MinimumLength &&
               text.Length <= MaximumLength &&
               text.StartsWith(Prefix, StringComparison.Ordinal);
    }

    internal bool CanUseForFilename(Barcode barcode)
    {
        if (!barcode.IsDetected || barcode.DetectedText == null)
        {
            return false;
        }
        if (BarcodeType == null && BarcodeTypes.Count == 0)
        {
            return true;
        }
        return barcode.DetectedFormat is { } format &&
               (BarcodeType == format || BarcodeTypes.Contains(format)) ||
               barcode.PatchCode is { } patchCode && PatchCodes.Contains(patchCode);
    }
}
