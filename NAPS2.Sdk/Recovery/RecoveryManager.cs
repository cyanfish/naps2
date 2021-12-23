using System.Collections.Immutable;
using System.Windows.Forms;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Serialization;
using NAPS2.WinForms;

namespace NAPS2.Recovery;

public class RecoveryManager
{
    private const string LOCK_FILE_NAME = ".lock";

    private readonly ScanningContext _scanningContext;
    private readonly ImageContext _imageContext;
    private readonly IFormFactory _formFactory;
    private readonly OperationProgress _operationProgress;

    public RecoveryManager(ScanningContext scanningContext, ImageContext imageContext, IFormFactory formFactory, OperationProgress operationProgress)
    {
        _scanningContext = scanningContext;
        _imageContext = imageContext;
        _formFactory = formFactory;
        _operationProgress = operationProgress;
    }

    public void RecoverScannedImages(Action<RenderableImage> imageCallback, RecoveryParams recoveryParams)
    {
        var op = new RecoveryOperation(_scanningContext, _imageContext, _formFactory);
        if (op.Start(imageCallback, recoveryParams))
        {
            _operationProgress.ShowProgress(op);
        }
    }

    private class RecoveryOperation : OperationBase
    {
        private readonly ScanningContext _scanningContext;
        private readonly ImageContext _imageContext;
        private readonly IFormFactory _formFactory;

        private FileStream? _lockFile;
        private DirectoryInfo? _folderToRecoverFrom;
        private RecoveryIndex? _recoveryIndex;
        private int _imageCount;
        private DateTime _scannedDateTime;

        public RecoveryOperation(ScanningContext scanningContext, ImageContext imageContext, IFormFactory formFactory)
        {
            _scanningContext = scanningContext;
            _imageContext = imageContext;
            _formFactory = formFactory;

            ProgressTitle = MiscResources.ImportProgress;
            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(Action<RenderableImage> imageCallback, RecoveryParams recoveryParams)
        {
            Status = new OperationStatus
            {
                StatusText = MiscResources.Recovering
            };

            _folderToRecoverFrom = FindAndLockFolderToRecoverFrom();
            if (_folderToRecoverFrom == null)
            {
                return false;
            }
            try
            {
                var serializer = new XmlSerializer<RecoveryIndex>();
                _recoveryIndex = serializer.DeserializeFromFile(Path.Combine(_folderToRecoverFrom.FullName, "index.xml"));
                _imageCount = _recoveryIndex.Images.Count;
                _scannedDateTime = _folderToRecoverFrom.LastWriteTime;
                if (_imageCount == 0)
                {
                    // If there are no images, do nothing. Don't delete the folder in case the index was corrupted somehow.
                    ReleaseFolderLock();
                    return false;
                }
                switch (PromptToRecover())
                {
                    case DialogResult.Yes: // Recover
                        RunAsync(async () =>
                        {
                            try
                            {
                                if (await DoRecover(imageCallback, recoveryParams))
                                {
                                    ReleaseFolderLock();
                                    DeleteFolder();
                                    return true;
                                }
                                return false;
                            }
                            finally
                            {
                                ReleaseFolderLock();
                                GC.Collect();
                            }
                        });
                        return true;
                    case DialogResult.No: // Delete
                        ReleaseFolderLock();
                        DeleteFolder();
                        break;
                    default: // Not Now
                        ReleaseFolderLock();
                        break;
                }
            }
            catch (Exception ex)
            {
                ReleaseFolderLock();
                Log.ErrorException("Could not open recovery folder.", ex);
            }
            return false;
        }

        private async Task<bool> DoRecover(Action<RenderableImage> imageCallback, RecoveryParams recoveryParams)
        {
            Status.MaxProgress = _recoveryIndex.Images.Count;
            InvokeStatusChanged();

            foreach (RecoveryIndexImage indexImage in _recoveryIndex.Images)
            {
                if (CancelToken.IsCancellationRequested)
                {
                    return false;
                }

                string imagePath = Path.Combine(_folderToRecoverFrom.FullName, indexImage.FileName);
                // TODO use UnownedFileStorage
                var transformState = new TransformState(indexImage.TransformList.ToImmutableList());
                RenderableImage renderableImage;
                if (".pdf".Equals(Path.GetExtension(imagePath), StringComparison.InvariantCultureIgnoreCase))
                {
                    string newPath = _scanningContext.FileStorageManager.NextFilePath() + ".pdf";
                    File.Copy(imagePath, newPath);
                    // TODO: Some kind of factory for pdf renderable image creation and default settings
                    renderableImage = new RenderableImage(new FileStorage(newPath), new ImageMetadata(BitDepth.Color, false), transformState);
                }
                else
                {
                    using var image = _imageContext.Load(imagePath);
                    renderableImage = new RenderableImage(image, new ImageMetadata(indexImage.BitDepth.ToBitDepth(), indexImage.HighQuality), transformState);
                }

                if (recoveryParams.ThumbnailSize.HasValue)
                {
                    renderableImage.PostProcessingData.Thumbnail = _imageContext.PerformTransform(renderableImage.RenderToImage(), new ThumbnailTransform(recoveryParams.ThumbnailSize.Value));
                }

                imageCallback(renderableImage);

                Status.CurrentProgress++;
                InvokeStatusChanged();
            }
            return true;
        }

        private DialogResult PromptToRecover()
        {
            var recoveryPromptForm = _formFactory.Create<FRecover>();
            recoveryPromptForm.SetData(_imageCount, _scannedDateTime);
            return recoveryPromptForm.ShowDialog();
        }

        private void DeleteFolder()
        {
            try
            {
                _folderToRecoverFrom.Delete(true);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error deleting recovery folder.", ex);
            }
        }

        private DirectoryInfo FindAndLockFolderToRecoverFrom()
        {
            // Find the most recent recovery folder that can be locked (i.e. isn't in use already)
            return new DirectoryInfo(Paths.Recovery)
                .EnumerateDirectories()
                .OrderByDescending(x => x.LastWriteTime)
                .FirstOrDefault(TryLockRecoveryFolder);
        }

        private void ReleaseFolderLock()
        {
            // Unlock the recover folder
            if (_lockFile != null)
            {
                _lockFile.Dispose();
                _lockFile = null;
            }
        }

        private bool TryLockRecoveryFolder(DirectoryInfo recoveryFolder)
        {
            try
            {
                string lockFilePath = Path.Combine(recoveryFolder.FullName, LOCK_FILE_NAME);
                _lockFile = new FileStream(lockFilePath, FileMode.Open);
                return true;
            }
            catch (Exception)
            {
                // Some problem, e.g. the folder is already locked
                return false;
            }
        }
    }
}