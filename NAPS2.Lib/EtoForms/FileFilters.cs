using Eto.Forms;

namespace NAPS2.EtoForms;

public class FileFilters
{
    private readonly ImageContext _imageContext;

    public FileFilters(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

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
        if (groups.HasFlag(FileFilterGroup.AllImages))
        {
            filters.Add(new FileFilter(MiscResources.FileTypeImageFiles,
                ".bmp", "jpg", ".jpeg", ".png", ".tiff", ".tif"));
        }
        if (groups.HasFlag(FileFilterGroup.Image))
        {
            filters.Add(new FileFilter(MiscResources.FileTypeBmp, ".bmp"));
            filters.Add(new FileFilter(MiscResources.FileTypeJpeg, ".jpg", ".jpeg"));
            if (_imageContext.SupportsFormat(ImageFileFormat.Jpeg2000))
            {
                filters.Add(new FileFilter(MiscResources.FileTypeJp2, ".jp2", ".jpx"));
            }
            filters.Add(new FileFilter(MiscResources.FileTypePng, ".png"));
            filters.Add(new FileFilter(MiscResources.FileTypeTiff, ".tiff", ".tif"));
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