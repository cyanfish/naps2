using NAPS2.Images;
using NAPS2.Serialization;
using ZXing;

namespace NAPS2.ImportExport;

/// <summary>
/// Defines which decoded barcodes separate documents and whether the separator page is retained.
/// </summary>
[GeneratePropertyAccessors]
public record BarcodeSeparatorOptions
{
    /// <summary>
    /// The patch codes that separate documents. If no patch codes or barcode types are specified, any barcode does.
    /// </summary>
    public List<PatchCodeType> PatchCodes { get; init; } = [];

    /// <summary>
    /// A single barcode format that separates documents, in addition to <see cref="BarcodeTypes"/>.
    /// </summary>
    public BarcodeFormat? BarcodeType { get; init; }

    /// <summary>
    /// The barcode formats that separate documents. If no patch codes or barcode types are specified, any barcode does.
    /// </summary>
    public List<BarcodeFormat> BarcodeTypes { get; init; } = [];

    /// <summary>
    /// The minimum length of the barcode text for it to separate documents.
    /// </summary>
    public int MinimumLength { get; init; } = 1;

    /// <summary>
    /// The maximum length of the barcode text for it to separate documents.
    /// </summary>
    public int MaximumLength { get; init; } = int.MaxValue;

    /// <summary>
    /// The text the barcode must start with for it to separate documents.
    /// </summary>
    public string Prefix { get; init; } = "";

    /// <summary>
    /// Whether the separator page is removed rather than kept as the first page of the next document.
    /// </summary>
    public bool RemoveBarcodePage { get; init; }

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
