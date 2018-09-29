using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.ImportExport.Email;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Ocr;
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
        private readonly IEmailProviderFactory emailProviderFactory;

        public SavePdfOperation(FileNamePlaceholders fileNamePlaceholders, IPdfExporter pdfExporter, IOverwritePrompt overwritePrompt, IEmailProviderFactory emailProviderFactory)
        {
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.pdfExporter = pdfExporter;
            this.overwritePrompt = overwritePrompt;
            this.emailProviderFactory = emailProviderFactory;

            AllowCancel = true;
            AllowBackground = true;
        }

        public bool Start(string fileName, DateTime dateTime, ICollection<ScannedImage> images, PdfSettings pdfSettings, OcrParams ocrParams, bool email, EmailMessage emailMessage)
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
            RunAsync(async () =>
            {
                bool result = false;
                try
                {
                    result = await pdfExporter.Export(subFileName, snapshots, pdfSettings, ocrParams, OnProgress, CancelToken);
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
                    snapshots.ForEach(s => s.Dispose());
                    GC.Collect();
                }
                
                if (result && email && emailMessage != null)
                {
                    Status.StatusText = MiscResources.UploadingEmail;
                    Status.CurrentProgress = 0;
                    Status.MaxProgress = 1;
                    Status.ProgressType = OperationProgressType.MB;
                    InvokeStatusChanged();

                    try
                    {
                        result = await emailProviderFactory.Default.SendEmail(emailMessage, OnProgress, CancelToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorException(MiscResources.ErrorEmailing, ex);
                        InvokeError(MiscResources.ErrorEmailing, ex);
                    }
                }

                return result;
            });
            Success.ContinueWith(task =>
            {
                if (task.Result)
                {
                    if (email)
                    {
                        Log.Event(EventType.Email, new EventParams
                        {
                            Name = MiscResources.EmailPdf,
                            Pages = snapshots.Count,
                            FileFormat = ".pdf"
                        });
                    }
                    else
                    {
                        Log.Event(EventType.SavePdf, new EventParams
                        {
                            Name = MiscResources.SavePdf,
                            Pages = snapshots.Count,
                            FileFormat = ".pdf"
                        });
                    }
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            return true;
        }
    }
}
