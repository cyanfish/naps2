namespace NAPS2.Scan;

public record MetadataCaps(
    string? DriverSubtype = null,
    string? Manufacturer = null,
    string? Model = null,
    string? SerialNumber = null,
    string? Location = null,
    string? IconUri = null
);