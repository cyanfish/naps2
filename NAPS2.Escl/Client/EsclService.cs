using System.Net;

namespace NAPS2.Escl.Client;

public class EsclService
{
    /// <summary>
    /// The IP (v4) address of the scanner. At least one of IpV4 and IpV6 will be non-null.
    /// </summary>
    public required IPAddress? IpV4 { get; init; }

    /// <summary>
    /// The IP (v6) address of the scanner. At least one of IpV4 and IpV6 will be non-null.
    /// </summary>
    public required IPAddress? IpV6 { get; init; }

    /// <summary>
    /// The port of the ESCL service.
    /// </summary>
    public required int Port { get; init; }

    /// <summary>
    /// Whether to use HTTPS for the connection.
    /// </summary>
    public required bool Tls { get; init; }

    /// <summary>
    /// The root path of the ESCL URLs with no leading or trailing slash. For example, "eSCL" means we would use a URL
    /// like "http://192.168.1.111:80/eSCL/ScannerCapabilities".
    /// </summary>
    public required string RootUrl { get; init; }

    /// <summary>
    /// A unique identifier for the physical scanner device.
    /// </summary>
    public string? Uuid { get; init; }

    /// <summary>
    /// The make and model of the scanner.
    /// </summary>
    public string? ScannerName { get; init; }

    /// <summary>
    /// The version of the TXT record the information in this class came from.
    /// </summary>
    public string? TxtVersion { get; init; }

    /// <summary>
    /// The configuration URL for the scanner.
    /// </summary>
    public string? AdminUrl { get; init; }

    /// <summary>
    /// The ESCL protocol version. Should be 2.0.
    /// </summary>
    public string? EsclVersion { get; init; }

    /// <summary>
    /// A URL to an image representing the scanner.
    /// </summary>
    public string? Thumbnail { get; init; }

    /// <summary>
    /// Extra information about the scanner's location (e.g. "3rd Floor Copy Room").
    /// </summary>
    public string? Note { get; init; }

    /// <summary>
    /// The MIME types supported by the scanner (e.g. "application/pdf", "image/jpeg").
    /// </summary>
    public string[]? MimeTypes { get; init; }

    /// <summary>
    /// Supported color options (e.g. "color", "grayscale", "binary").
    /// </summary>
    public string[]? ColorOptions { get; init; }

    /// <summary>
    /// Supported input source options (e.g. "platen", "adf", "camera").
    /// </summary>
    public string? SourceOptions { get; init; }

    /// <summary>
    /// Whether duplex is supported with the "adf" source.
    /// </summary>
    public bool? DuplexSupported { get; init; }
}