using NAPS2.Serialization;

namespace NAPS2.ImportExport.Email;

[GeneratePropertyAccessors]
public record EmailSettings
{
    public string AttachmentName { get; init; } = "Scan.pdf";
}