namespace NAPS2.Scan;

/// <summary>
/// Options for detecting barcodes using ZXing.
/// </summary>
public class BarcodeDetectionOptions
{
    public bool DetectBarcodes { get; set; }
        
    public bool PatchTOnly { get; set; }

    /// <summary>
    /// Image is a pure monochrome image of a barcode.
    /// </summary>
    /// <value>
    ///   <c>true</c> if monochrome image of a barcode; otherwise, <c>false</c>.
    /// </value>
    public bool PureBarcode { get; set; }

    /// <summary>
    /// Specifies what character encoding to use when decoding, where applicable (type String)
    /// </summary>
    /// <value>
    /// The character set.
    /// </value>
    public string? CharacterSet { get; set; }

    /// <summary>
    /// Image is known to be of one of a few possible formats.
    /// Bitfield enum of {@link BarcodeFormat}s.
    /// </summary>
    /// <value>
    /// The possible formats.
    /// </value>
    public BarcodeFormat PossibleFormats { get; set; }

    /// <summary>
    /// if Code39 could be detected try to use extended mode for full ASCII character set
    /// </summary>
    public bool UseCode39ExtendedMode { get; set; }

    /// <summary>
    /// Don't fail if a Code39 is detected but can't be decoded in extended mode.
    /// Return the raw Code39 result instead. Maps to <see cref="bool" />.
    /// </summary>
    public bool UseCode39RelaxedExtendedMode { get; set; }

    /// <summary>
    /// Assume Code 39 codes employ a check digit. Maps to <see cref="bool" />.
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should assume a Code 39 check digit; otherwise, <c>false</c>.
    /// </value>
    public bool AssumeCode39CheckDigit { get; set; }

    /// <summary>
    /// If true, return the start and end digits in a Codabar barcode instead of stripping them. They
    /// are alpha, whereas the rest are numeric. By default, they are stripped, but this causes them
    /// to not be. Doesn't matter what it maps to; use <see cref="bool" />.
    /// </summary>
    public bool ReturnCodabarStartEnd { get; set; }

    /// <summary>
    /// Assume the barcode is being processed as a GS1 barcode, and modify behavior as needed.
    /// For example this affects FNC1 handling for Code 128 (aka GS1-128).
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should assume GS1; otherwise, <c>false</c>.
    /// </value>
    public bool AssumeGS1 { get; set; }

    /// <summary>
    /// Assume MSI codes employ a check digit. Maps to <see cref="bool" />.
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should assume a MSI check digit; otherwise, <c>false</c>.
    /// </value>
    public bool AssumeMSICheckDigit { get; set; }

    /// <summary>
    /// Allowed lengths of encoded data -- reject anything else. Maps to an int[].
    /// </summary>
    public int[]? AllowedLengths { get; set; }

    /// <summary>
    /// Allowed extension lengths for EAN or UPC barcodes. Other formats will ignore this.
    /// Maps to an int[] of the allowed extension lengths, for example [2], [5], or [2, 5].
    /// If it is optional to have an extension, do not set this hint. If this is set,
    /// and a UPC or EAN barcode is found but an extension is not, then no result will be returned
    /// at all.
    /// </summary>
    public int[]? AllowedEANExtensions { get; set; }

}

//
// Summary:
//     Enumerates barcode formats known to this package.
//    
[Flags]
public enum BarcodeFormat
{
    //
    // Summary:
    //     Aztec 2D barcode format.
    AZTEC = 1,
    //
    // Summary:
    //     CODABAR 1D format.
    CODABAR = 2,
    //
    // Summary:
    //     Code 39 1D format.
    CODE_39 = 4,
    //
    // Summary:
    //     Code 93 1D format.
    CODE_93 = 8,
    //
    // Summary:
    //     Code 128 1D format.
    CODE_128 = 0x10,
    //
    // Summary:
    //     Data Matrix 2D barcode format.
    DATA_MATRIX = 0x20,
    //
    // Summary:
    //     EAN-8 1D format.
    EAN_8 = 0x40,
    //
    // Summary:
    //     EAN-13 1D format.
    EAN_13 = 0x80,
    //
    // Summary:
    //     ITF (Interleaved Two of Five) 1D format.
    ITF = 0x100,
    //
    // Summary:
    //     MaxiCode 2D barcode format.
    MAXICODE = 0x200,
    //
    // Summary:
    //     PDF417 format.
    PDF_417 = 0x400,
    //
    // Summary:
    //     QR Code 2D barcode format.
    QR_CODE = 0x800,
    //
    // Summary:
    //     RSS 14
    RSS_14 = 0x1000,
    //
    // Summary:
    //     RSS EXPANDED
    RSS_EXPANDED = 0x2000,
    //
    // Summary:
    //     UPC-A 1D format.
    UPC_A = 0x4000,
    //
    // Summary:
    //     UPC-E 1D format.
    UPC_E = 0x8000,
    //
    // Summary:
    //     UPC/EAN extension format. Not a stand-alone format.
    UPC_EAN_EXTENSION = 0x10000,
    //
    // Summary:
    //     MSI
    MSI = 0x20000,
    //
    // Summary:
    //     Plessey
    PLESSEY = 0x40000,
    //
    // Summary:
    //     Intelligent Mail barcode
    IMB = 0x80000,
    //
    // Summary:
    //     Pharmacode format.
    PHARMA_CODE = 0x100000,
    //
    // Summary:
    //     UPC_A | UPC_E | EAN_13 | EAN_8 | CODABAR | CODE_39 | CODE_93 | CODE_128 | ITF
    //     | RSS_14 | RSS_EXPANDED without MSI (to many false-positives) and IMB (not enough
    //     tested, and it looks more like a 2D)
    All_1D = 0xF1DE
}