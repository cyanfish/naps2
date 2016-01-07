using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Recovery
{
    public class RecoveryManager
    {
        private readonly IFormFactory formFactory;
        private readonly IScannedImageFactory scannedImageFactory;
        private readonly IUserConfigManager userConfigManager;
        private readonly ThreadFactory threadFactory;

        public RecoveryManager(IFormFactory formFactory, IScannedImageFactory scannedImageFactory, IUserConfigManager userConfigManager, ThreadFactory threadFactory)
        {
            this.formFactory = formFactory;
            this.scannedImageFactory = scannedImageFactory;
            this.userConfigManager = userConfigManager;
            this.threadFactory = threadFactory;
        }

        public void RecoverScannedImages(Action<IScannedImage> imageCallback)
        {
            var op = new RecoveryOperation(formFactory, scannedImageFactory, userConfigManager, threadFactory);
            var progressForm = formFactory.Create<FProgress>();
            progressForm.Operation = op;
            if (op.Start(imageCallback))
            {
                progressForm.ShowDialog();
            }
        }

        private class RecoveryOperation : OperationBase
        {
            private readonly IFormFactory formFactory;
            private readonly IScannedImageFactory scannedImageFactory;
            private readonly IUserConfigManager userConfigManager;
            private readonly ThreadFactory threadFactory;

            private FileStream lockFile;
            private DirectoryInfo folderToRecoverFrom;
            private RecoveryIndexManager recoveryIndexManager;
            private int imageCount;
            private DateTime scannedDateTime;
            private bool cancel;

            public RecoveryOperation(IFormFactory formFactory, IScannedImageFactory scannedImageFactory, IUserConfigManager userConfigManager, ThreadFactory threadFactory)
            {
                this.formFactory = formFactory;
                this.scannedImageFactory = scannedImageFactory;
                this.userConfigManager = userConfigManager;
                this.threadFactory = threadFactory;

                ProgressTitle = MiscResources.ImportProgress;
                AllowCancel = true;
            }

            public bool Start(Action<IScannedImage> imageCallback)
            {
                Status = new OperationStatus
                {
                    StatusText = MiscResources.Recovering
                };
                cancel = false;

                folderToRecoverFrom = FindAndLockFolderToRecoverFrom();
                if (folderToRecoverFrom == null)
                {
                    return false;
                }
                try
                {
                    recoveryIndexManager = new RecoveryIndexManager(folderToRecoverFrom);
                    imageCount = recoveryIndexManager.Index.Images.Count;
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
                            threadFactory.StartThread(() =>
                            {
                                try
                                {
                                    if (DoRecover(imageCallback))
                                    {
                                        ReleaseFolderLock();
                                        DeleteFolder();
                                        Status.Success = true;
                                    }
                                }
                                finally
                                {
                                    ReleaseFolderLock();
                                    InvokeFinished();
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
                catch (Exception)
                {
                    ReleaseFolderLock();
                    throw;
                }
                return false;
            }

            private bool DoRecover(Action<IScannedImage> imageCallback)
            {
                Status.MaxProgress = recoveryIndexManager.Index.Images.Count;
                InvokeStatusChanged();

                foreach (RecoveryIndexImage indexImage in recoveryIndexManager.Index.Images)
                {
                    if (cancel)
                    {
                        return false;
                    }

                    string imagePath = Path.Combine(folderToRecoverFrom.FullName, indexImage.FileName);
                    using (var bitmap = new Bitmap(imagePath))
                    {
                        var scannedImage = scannedImageFactory.Create(bitmap, indexImage.BitDepth,
                            indexImage.HighQuality);
                        foreach (var transform in indexImage.TransformList)
                        {
                            scannedImage.AddTransform(transform);
                        }
                        scannedImage.SetThumbnail(
                            scannedImage.RenderThumbnail(userConfigManager.Config.ThumbnailSize));
                        imageCallback(scannedImage);
                    }

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
                    string lockFilePath = Path.Combine(recoveryFolder.FullName, FileBasedScannedImage.LOCK_FILE_NAME);
                    lockFile = new FileStream(lockFilePath, FileMode.Open);
                    return true;
                }
                catch (Exception)
                {
                    // Some problem, e.g. the folder is already locked
                    return false;
                }
            }

            public override void Cancel()
            {
                cancel = true;
            }
        }
    }
}
