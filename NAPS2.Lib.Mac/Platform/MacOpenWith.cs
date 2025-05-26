using NAPS2.Images.Mac;
using UniformTypeIdentifiers;

namespace NAPS2.Platform;

public class MacOpenWith : IOpenWith
{
    public IEnumerable<OpenWithEntry> GetEntries(string fileExt)
    {
        UTType contentType = fileExt switch
        {
            ".jpg" => UTTypes.Jpeg,
            _ => throw new NotSupportedException("Unsupported mime type/extension")
        };
        var appUrls = NSWorkspace.SharedWorkspace.GetUrlsForApplicationsToOpenContentType(contentType);
        foreach (var appUrl in appUrls)
        {
            yield return new OpenWithEntry(
                appUrl.Path!,
                Path.GetFileNameWithoutExtension(appUrl.Path!),
                "",
                0
            );
        }
    }

    public void OpenWith(string entryId, string filePath)
    {
        Process.Start("open", $"-a \"{entryId}\" \"{filePath}\"");
    }

    public IMemoryImage LoadIcon(OpenWithEntry entry)
    {
        NSImage allReps = NSWorkspace.SharedWorkspace.IconForFile(entry.Id);
        // TODO: Any cleaner way to do this conversion?
        NSImageRep rep = allReps.BestRepresentation(new CGRect(0, 0, 64, 64), null, null);
        NSImage image = new NSImage();
        image.AddRepresentation(new NSBitmapImageRep(rep.CGImage));
        return new MacImage(image);
    }
}