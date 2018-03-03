using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Recovery;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.Worker;

namespace NAPS2.ImportExport.Pdf
{
    public class SavePdfOperation : WorkerOperation
    {
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly IPdfExporter pdfExporter;
        private readonly IOverwritePrompt overwritePrompt;
        private readonly ThreadFactory threadFactory;
        private readonly AppConfigManager appConfigManager;
        
        private Thread thread;

        public SavePdfOperation(FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOverwritePrompt overwritePrompt, ThreadFactory threadFactory, AppConfigManager appConfigManager, IWorkerServiceFactory workerServiceFactory)
            : base(workerServiceFactory)
        {
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.overwritePrompt = overwritePrompt;
            this.threadFactory = threadFactory;
            this.appConfigManager = appConfigManager;

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

            var snapshots = images.Select(x => x.Preserve()).ToList();
            thread = threadFactory.StartThread(() =>
            {
                try
                {
                    if (UseWorker)
                    {
                        using (var worker = WorkerServiceFactory.Create())
                        {
                            worker.Service.SetRecoveryFolder(RecoveryImage.RecoveryFolder.FullName);
                            worker.Callback.OnProgress += OnProgress;
                            worker.Service.ExportPdf(subFileName, snapshots.Select(ScannedImage.Snapshot.Export).ToList(), pdfSettings, ocrLanguageCode);
                            Status.Success = worker.Callback.WaitForFinish();
                        }
                    }
                    else
                    {
                        Status.Success = pdfExporter.Export(subFileName, snapshots, pdfSettings, ocrLanguageCode, OnProgress);
                    }
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
                finally
                {
                    foreach (var s in snapshots)
                    {
                        s.Dispose();
                    }
                }
                GC.Collect();
                InvokeFinished();
            });

            return true;
        }

        public override void WaitUntilFinished()
        {
            thread.Join();
        }
    }
}
