namespace NAPS2.ImportExport;

public class ImportOperation : OperationBase
{
    private readonly IFileImporter _fileImporter;

    public ImportOperation(IFileImporter fileImporter)
    {
        _fileImporter = fileImporter;

        ProgressTitle = MiscResources.ImportProgress;
        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(List<string> filesToImport, Action<ProcessedImage> imageCallback, ImportParams importParams)
    {
        bool oneFile = filesToImport.Count == 1;
        Status = new OperationStatus
        {
            MaxProgress = oneFile ? 0 : filesToImport.Count
        };

        RunAsync(async () =>
        {
            try
            {
                foreach (var fileName in filesToImport)
                {
                    try
                    {
                        Status.StatusText = string.Format(MiscResources.ImportingFormat, Path.GetFileName(fileName));
                        InvokeStatusChanged();
                        var images = _fileImporter.Import(fileName, importParams, oneFile ? ProgressHandler : CancelToken);
                        await foreach (var image in images)
                        {
                            imageCallback(image);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(fileName)), ex);
                        InvokeError(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(fileName)), ex);
                    }
                    if (!oneFile)
                    {
                        Status.CurrentProgress++;
                        InvokeStatusChanged();
                    }
                }
                return true;
            }
            finally
            {
                GC.Collect();
            }
        });
        return true;
    }
}