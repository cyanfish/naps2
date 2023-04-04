namespace NAPS2.EtoForms.Notifications;

/// <summary>
/// A base interface for objects that can display information about saved files to the user.
/// </summary>
public interface ISaveNotify
{
    /// <summary>
    /// Indicate that a PDF file has been saved.
    /// </summary>
    /// <param name="path"></param>
    void PdfSaved(string path);

    /// <summary>
    /// Indicate that one or more image files have been saved.
    /// </summary>
    /// <param name="imageCount"></param>
    /// <param name="path"></param>
    void ImagesSaved(int imageCount, string path);
}