using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Scan;
using NAPS2.Serialization;
using NAPS2.WinForms;

namespace NAPS2.Recovery
{
    public class RecoveryManager
    {
        private readonly ImageContext _imageContext;
        private readonly IFormFactory _formFactory;
        private readonly ImageRenderer _imageRenderer;
        private readonly OperationProgress _operationProgress;

        public RecoveryManager(ImageContext imageContext, IFormFactory formFactory, ImageRenderer imageRenderer, OperationProgress operationProgress)
        {
            _imageContext = imageContext;
            _formFactory = formFactory;
            _imageRenderer = imageRenderer;
            _operationProgress = operationProgress;
        }

        public void RecoverScannedImages(Action<ScannedImage> imageCallback, RecoveryParams recoveryParams)
        {
            var op = new RecoveryOperation(_imageContext, _formFactory, _imageRenderer);
            if (op.Start(imageCallback, recoveryParams))
            {
                _operationProgress.ShowProgress(op);
            }
        }

        private class RecoveryOperation : OperationBase
        {
            private readonly ImageContext _imageContext;
            private readonly IFormFactory _formFactory;
            private readonly ImageRenderer _imageRenderer;

            private FileStream? _lockFile;
            private DirectoryInfo? _folderToRecoverFrom;
            private RecoveryIndex? _recoveryIndex;
            private int _imageCount;
            private DateTime _scannedDateTime;

            public RecoveryOperation(ImageContext imageContext, IFormFactory formFactory, ImageRenderer imageRenderer)
            {
                _imageContext = imageContext;
                _formFactory = formFactory;
                _imageRenderer = imageRenderer;

                ProgressTitle = MiscResources.ImportProgress;
                AllowCancel = true;
                AllowBackground = true;
            }

            public bool Start(Action<ScannedImage> imageCallback, RecoveryParams recoveryParams)
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

            private async Task<bool> DoRecover(Action<ScannedImage> imageCallback, RecoveryParams recoveryParams)
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
                    ScannedImage scannedImage;
                    if (".pdf".Equals(Path.GetExtension(imagePath), StringComparison.InvariantCultureIgnoreCase))
                    {
                        string newPath = _imageContext.FileStorageManager.NextFilePath() + ".pdf";
                        File.Copy(imagePath, newPath);
                        scannedImage = _imageContext.CreateScannedImage(new FileStorage(newPath));
                    }
                    else
                    {
                        using var bitmap = _imageContext.ImageFactory.Decode(imagePath);
                        scannedImage = _imageContext.CreateScannedImage(bitmap, indexImage.BitDepth.ToBitDepth(), indexImage.HighQuality, -1);
                    }
                    foreach (var transform in indexImage.TransformList)
                    {
                        scannedImage.AddTransform(transform);
                    }

                    if (recoveryParams.ThumbnailSize.HasValue)
                    {
                        scannedImage.SetThumbnail(_imageContext.PerformTransform(await _imageRenderer.Render(scannedImage), new ThumbnailTransform(recoveryParams.ThumbnailSize.Value)));
                    }

                    imageCallback(scannedImage);

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
                    string lockFilePath = Path.Combine(recoveryFolder.FullName, RecoveryStorageManager.LOCK_FILE_NAME);
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
}
