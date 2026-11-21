using Eto.Forms;

namespace NAPS2.EtoForms;

public class FileFilters
{
    private readonly ImageContext _imageContext;

    public FileFilters(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    private static (ImageFileFormat format, string name, string[] extensions)[] ImageFilters =>
    [
        (ImageFileFormat.Avif, MiscResources.FileTypeAvif, [".avif"]),
        (ImageFileFormat.Bmp, MiscResources.FileTypeBmp, [".bmp"]),
        (ImageFileFormat.Gif, MiscResources.FileTypeGif, [".gif"]),
        (ImageFileFormat.Heic, MiscResources.FileTypeHeic, [".heic", ".heif"]),
        (ImageFileFormat.Jpeg, MiscResources.FileTypeJpeg, [".jpg", ".jpeg"]),
        (ImageFileFormat.Jpeg2000, MiscResources.FileTypeJp2, [".jp2", ".jpx"]),
        (ImageFileFormat.JpegXl, MiscResources.FileTypeJxl, [".jxl"]),
        (ImageFileFormat.Png, MiscResources.FileTypePng, [".png"]),
        (ImageFileFormat.Tiff, MiscResources.FileTypeTiff, [".tiff", ".tif"]),
        (ImageFileFormat.WebP, MiscResources.FileTypeWebp, [".webp"])
    ];

    public void Set(FileDialog fileDialog, FileFilterGroup groups, string? selectedExt = null)
    {
        var filters = fileDialog.Filters;
        if (groups.HasFlag(FileFilterGroup.AllFiles))
        {
            filters.Add(new FileFilter(MiscResources.FileTypeAllFiles, ".*"));
        }
        if (groups.HasFlag(FileFilterGroup.Pdf))
        {
            filters.Add(new FileFilter(MiscResources.FileTypePdf, ".pdf"));
        }
        Func<ImageFileFormat, bool> isSupported = fileDialog is SaveFileDialog
            ? _imageContext.SupportsEncode
            : _imageContext.SupportsDecode;
        var imageFilters = ImageFilters.Where(x => isSupported(x.format)).ToList();
        if (groups.HasFlag(FileFilterGroup.AllImages))
        {
            filters.Add(new FileFilter(MiscResources.FileTypeImageFiles,
                imageFilters.SelectMany(x => x.extensions).ToArray()));
        }
        if (groups.HasFlag(FileFilterGroup.Image))
        {
            foreach (var (_, name, extensions) in imageFilters)
            {
                filters.Add(new FileFilter(name, extensions));
            }
        }
        // TODO: Fix setting current filter on Eto GTK
        if (selectedExt != null && !EtoPlatform.Current.IsGtk)
        {
            selectedExt = selectedExt.Replace(".", "");
            foreach (var filter in filters)
            {
                if (filter.Extensions.Any(x => x.Replace(".", "") == selectedExt))
                {
                    fileDialog.CurrentFilter = filter;
                }
            }
        }
    }
}