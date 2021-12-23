namespace NAPS2.ImportExport;

public class ImportOperation : OperationBase
{
    private readonly IScannedImageImporter _scannedImageImporter;

    public ImportOperation(IScannedImageImporter scannedImageImporter)
    {
        _scannedImageImporter = scannedImageImporter;

        ProgressTitle = MiscResources.ImportProgress;
        AllowCancel = true;
        AllowBackground = true;
    }

    public bool Start(List<string> filesToImport, Action<RenderableImage> imageCallback, ImportParams importParams)
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
                        var imageSrc = _scannedImageImporter.Import(fileName, importParams, oneFile ? OnProgress : new ProgressHandler((j, k) => { }), CancelToken);
                        await imageSrc.ForEach(imageCallback);
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