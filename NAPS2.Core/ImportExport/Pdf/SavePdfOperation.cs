using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Pdf
{
    public class SavePdfOperation : OperationBase
    {
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly IPdfExporter pdfExporter;
        private readonly IOverwritePrompt overwritePrompt;
        private readonly ThreadFactory threadFactory;

        private bool cancel;
        private Thread thread;

        public SavePdfOperation(FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOverwritePrompt overwritePrompt, ThreadFactory threadFactory)
        {
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.overwritePrompt = overwritePrompt;
            this.threadFactory = threadFactory;

            AllowCancel = true;
        }

        public bool Start(string fileName, DateTime dateTime, ICollection<ScannedImage> images, PdfSettings pdfSettings, string ocrLanguageCode, bool email)
        {
            ProgressTitle = email ? MiscResources.EmailPdfProgress : MiscResources.SavePdfProgress;
            var subFileName = fileNamePlaceholders.SubstitutePlaceholders(fileName, dateTime);
            Status = new OperationStatus
            {
                StatusText = string.Format(MiscResources.SavingFormat, Path.GetFileName(subFileName)),
                MaxProgress = images.Count
            };
            cancel = false;

            if (Directory.Exists(subFileName))
            {
                // Not supposed to be a directory, but ok...
                subFileName = fileNamePlaceholders.SubstitutePlaceholders(Path.Combine(subFileName, "$(n).pdf"), dateTime);
            }
            if (File.Exists(subFileName))
            {
                if (overwritePrompt.ConfirmOverwrite(subFileName) != DialogResult.Yes)
                {
                    return false;
                }
            }

            thread = threadFactory.StartThread(() =>
            {
                try
                {
                    Status.Success = pdfExporter.Export(subFileName, images, pdfSettings, ocrLanguageCode, i =>
                    {
                        Status.CurrentProgress = i;
                        InvokeStatusChanged();
                        return !cancel;
                    });
                }
                catch (UnauthorizedAccessException ex)
                {
                    InvokeError(MiscResources.DontHavePermission, ex);
                }
                catch (IOException ex)
                {
                    if (File.Exists(subFileName))
                    {
                        InvokeError(MiscResources.FileInUse, ex);
                    }
                    else
                    {
                        Log.ErrorException(MiscResources.ErrorSaving, ex);
                        InvokeError(MiscResources.ErrorSaving, ex);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException(MiscResources.ErrorSaving, ex);
                    InvokeError(MiscResources.ErrorSaving, ex);
                }
                GC.Collect();
                InvokeFinished();
            });

            return true;
        }

        public override void Cancel()
        {
            cancel = true;
        }

        public override void WaitUntilFinished()
        {
            thread.Join();
        }
    }
}
