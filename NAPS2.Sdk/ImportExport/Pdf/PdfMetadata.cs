using NAPS2.Lang.Resources;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfMetadata
    {
        public PdfMetadata()
        {
            Title = MiscResources.ScannedImage;
            Subject = MiscResources.ScannedImage;
            Author = MiscResources.NAPS2;
            Creator = MiscResources.NAPS2;
        }

        public string Author { get; set; }
        public string Creator { get; set; }
        public string Keywords { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }
    }
}