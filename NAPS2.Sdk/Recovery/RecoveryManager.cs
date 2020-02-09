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
        private readonly ImageContext imageContext;
        private readonly IFormFactory formFactory;
        private readonly ImageRenderer imageRenderer;
        private readonly OperationProgress operationProgress;

        public RecoveryManager(ImageContext imageContext, IFormFactory formFactory, ImageRenderer imageRenderer, OperationProgress operationProgress)
        {
            this.imageContext = imageContext;
            this.formFactory = formFactory;
            this.imageRenderer = imageRenderer;
            this.operationProgress = operationProgress;
        }

        public void RecoverScannedImages(Action<ScannedImage> imageCallback, RecoveryParams recoveryParams)
        {
            var op = new RecoveryOperation(imageContext, formFactory, imageRenderer);
            if (op.Start(imageCallback, recoveryParams))
            {
                operationProgress.ShowProgress(op);
            }
        }

        private class RecoveryOperation : OperationBase
        {
            private readonly ImageContext imageContext;
            private readonly IFormFactory formFactory;
            private readonly ImageRenderer imageRenderer;

            private FileStream? lockFile;
            private DirectoryInfo? folderToRecoverFrom;
            private RecoveryIndex? recoveryIndex;
            private int imageCount;
            private DateTime scannedDateTime;

            public RecoveryOperation(ImageContext imageContext, IFormFactory formFactory, ImageRenderer imageRenderer)
            {
                this.imageContext = imageContext;
                this.formFactory = formFactory;
                this.imageRenderer = imageRenderer;

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

                folderToRecoverFrom = FindAndLockFolderToRecoverFrom();
                if (folderToRecoverFrom == null)
                {
                    return false;
                }
                try
                {
                    var serializer = new XmlSerializer<RecoveryIndex>();
                    recoveryIndex = serializer.DeserializeFromFile(Path.Combine(folderToRecoverFrom.FullName, "index.xml"));
                    imageCount = recoveryIndex.Images.Count;
                    scannedDateTime = folderToRecoverFrom.LastWriteTime;
                    if (imageCount == 0)
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
                Status.MaxProgress = recoveryIndex.Images.Count;
                InvokeStatusChanged();

                foreach (RecoveryIndexImage indexImage in recoveryIndex.Images)
                {
                    if (CancelToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    string imagePath = Path.Combine(folderToRecoverFrom.FullName, indexImage.FileName);
                    // TODO use UnownedFileStorage
                    ScannedImage scannedImage;
                    if (".pdf".Equals(Path.GetExtension(imagePath), StringComparison.InvariantCultureIgnoreCase))
                    {
                        string newPath = imageContext.FileStorageManager.NextFilePath() + ".pdf";
                        File.Copy(imagePath, newPath);
                        scannedImage = imageContext.CreateScannedImage(new FileStorage(newPath));
                    }
                    else
                    {
                        using var bitmap = imageContext.ImageFactory.Decode(imagePath);
                        scannedImage = imageContext.CreateScannedImage(bitmap, indexImage.BitDepth.ToBitDepth(), indexImage.HighQuality, -1);
                    }
                    foreach (var transform in indexImage.TransformList)
                    {
                        scannedImage.AddTransform(transform);
                    }

                    if (recoveryParams.ThumbnailSize.HasValue)
                    {
                        scannedImage.SetThumbnail(imageContext.PerformTransform(await imageRenderer.Render(scannedImage), new ThumbnailTransform(recoveryParams.ThumbnailSize.Value)));
                    }

                    imageCallback(scannedImage);

                    Status.CurrentProgress++;
                    InvokeStatusChanged();
                }
                return true;
            }

            private DialogResult PromptToRecover()
            {
                var recoveryPromptForm = formFactory.Create<FRecover>();
                recoveryPromptForm.SetData(imageCount, scannedDateTime);
                return recoveryPromptForm.ShowDialog();
            }

            private void DeleteFolder()
            {
                try
                {
                    folderToRecoverFrom.Delete(true);
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
                if (lockFile != null)
                {
                    lockFile.Dispose();
                    lockFile = null;
                }
            }

            private bool TryLockRecoveryFolder(DirectoryInfo recoveryFolder)
            {
                try
                {
                    string lockFilePath = Path.Combine(recoveryFolder.FullName, RecoveryStorageManager.LOCK_FILE_NAME);
                    lockFile = new FileStream(lockFilePath, FileMode.Open);
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
