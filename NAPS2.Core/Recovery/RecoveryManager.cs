using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan.Images;
using NAPS2.WinForms;

namespace NAPS2.Recovery
{
    public class RecoveryManager
    {
        private readonly IFormFactory formFactory;
        private readonly IScannedImageFactory scannedImageFactory;

        public RecoveryManager(IFormFactory formFactory, IScannedImageFactory scannedImageFactory)
        {
            this.formFactory = formFactory;
            this.scannedImageFactory = scannedImageFactory;
        }

        public IEnumerable<IScannedImage> RecoverScannedImages()
        {
            // Use an internal class with its own state so that RecoverManager is thread-safe
            return new RecoveryState(formFactory, scannedImageFactory).RecoverScannedImages();
        }

        private class RecoveryState
        {
            private readonly IFormFactory formFactory;
            private readonly IScannedImageFactory scannedImageFactory;

            private FileStream lockFile;
            private DirectoryInfo folderToRecoverFrom;
            private RecoveryIndexManager recoveryIndexManager;
            private int imageCount;
            private DateTime scannedDateTime;

            public RecoveryState(IFormFactory formFactory, IScannedImageFactory scannedImageFactory)
            {
                this.formFactory = formFactory;
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
                        var scannedImage = scannedImageFactory.Create(bitmap, indexImage.BitDepth, indexImage.HighQuality);
                        foreach (var transform in indexImage.TransformSet)
                        {
                            scannedImage.AddTransform(transform);
                        }
                        yield return scannedImage;
                    }
                }
            }

            private DialogResult PromptToRecover()
            {
                var recoveryPromptForm = formFactory.Create<FRecover>();
                recoveryPromptForm.SetData(imageCount, scannedDateTime);
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
                    Log.ErrorException("Error deleting recovery folder.", ex);
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
