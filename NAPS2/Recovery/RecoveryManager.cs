using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.WinForms;
using Ninject;
using Ninject.Parameters;
using NLog;

namespace NAPS2.Recovery
{
    public class RecoveryManager
    {
        private readonly IKernel kernel;
        private readonly Logger logger;
        private readonly IScannedImageFactory scannedImageFactory;

        public RecoveryManager(Logger logger, IKernel kernel, IScannedImageFactory scannedImageFactory)
        {
            this.logger = logger;
            this.kernel = kernel;
            this.scannedImageFactory = scannedImageFactory;
        }

        public IEnumerable<IScannedImage> RecoverScannedImages()
        {
            // Use an internal class with its own state so that RecoverManager is thread-safe
            return new RecoveryState(kernel, logger, scannedImageFactory).RecoverScannedImages();
        }

        private class RecoveryState
        {
            private readonly IKernel kernel;
            private readonly Logger logger;
            private readonly IScannedImageFactory scannedImageFactory;

            private FileStream lockFile;
            private DirectoryInfo folderToRecoverFrom;
            private RecoveryIndexManager recoveryIndexManager;
            private int imageCount;
            private DateTime scannedDateTime;

            public RecoveryState(IKernel kernel, Logger logger, IScannedImageFactory scannedImageFactory)
            {
                this.kernel = kernel;
                this.logger = logger;
                this.scannedImageFactory = scannedImageFactory;
            }

            public IEnumerable<IScannedImage> RecoverScannedImages()
            {
                StartRecovery();
                if (folderToRecoverFrom == null)
                {
                    yield break;
                }
                try
                {
                    recoveryIndexManager = new RecoveryIndexManager(folderToRecoverFrom);
                    imageCount = recoveryIndexManager.Index.Images.Count;
                    scannedDateTime = folderToRecoverFrom.LastWriteTime;
                    if (imageCount > 0)
                    {
                        // If there are no images, do nothing. Don't delete the folder in case the index was corrupted somehow.
                        switch (PromptToRecover())
                        {
                            case DialogResult.Yes:
                                foreach (var scannedImage in DoRecover())
                                {
                                    yield return scannedImage;
                                }
                                DeleteRecoveryFolder();
                                break;
                            case DialogResult.No:
                                DeleteRecoveryFolder();
                                break;
                        }
                    }
                }
                finally
                {
                    FinishRecovery();
                }
            }

            private IEnumerable<IScannedImage> DoRecover()
            {
                foreach (RecoveryIndexImage indexImage in recoveryIndexManager.Index.Images)
                {
                    string imagePath = Path.Combine(folderToRecoverFrom.FullName, indexImage.FileName);
                    using (var bitmap = new Bitmap(imagePath))
                    {
                        var scannedImage = scannedImageFactory.Create(bitmap,
                            (ScanBitDepth)indexImage.BitDepth, indexImage.HighQuality);
                        scannedImage.RotateFlip((RotateFlipType)indexImage.Transform);
                        yield return scannedImage;
                    }
                }
            }

            private DialogResult PromptToRecover()
            {
                var recoveryPromptForm = kernel.Get<FRecover>(new ConstructorArgument("imageCount", imageCount),
                    new ConstructorArgument("scannedDateTime", scannedDateTime));
                return recoveryPromptForm.ShowDialog();
            }

            private void DeleteRecoveryFolder()
            {
                // Release all locks so that the folder's deletion will work
                FinishRecovery();

                try
                {
                    folderToRecoverFrom.Delete(true);
                }
                catch (Exception ex)
                {
                    logger.ErrorException("Error deleting recovery folder.", ex);
                }
            }

            private void StartRecovery()
            {
                // Find the most recent recovery folder that can be locked (i.e. isn't in use already)
                folderToRecoverFrom = new DirectoryInfo(Paths.Recovery)
                    .EnumerateDirectories()
                    .OrderByDescending(x => x.LastWriteTime)
                    .FirstOrDefault(TryLockRecoveryFolder);
            }

            private void FinishRecovery()
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
        }
    }
}
