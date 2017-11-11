using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using NAPS2.Config;
using NAPS2.Dependencies;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Automation
{
    public class AutomatedScanning
    {
        private readonly IEmailer emailer;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IErrorOutput errorOutput;
        private readonly IScannedImageImporter scannedImageImporter;
        private readonly IUserConfigManager userConfigManager;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly IOperationFactory operationFactory;
        private readonly AppConfigManager appConfigManager;
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly IFormFactory formFactory;

        private readonly AutomatedScanningOptions options;
        private ScannedImageList imageList;
        private int pagesScanned;
        private int totalPagesScanned;
        private DateTime startTime;
        private string actualOutputPath;

        public AutomatedScanning(AutomatedScanningOptions options, IProfileManager profileManager, IScanPerformer scanPerformer, IErrorOutput errorOutput, IEmailer emailer, IScannedImageImporter scannedImageImporter, IUserConfigManager userConfigManager, PdfSettingsContainer pdfSettingsContainer, FileNamePlaceholders fileNamePlaceholders, ImageSettingsContainer imageSettingsContainer, IOperationFactory operationFactory, AppConfigManager appConfigManager, OcrDependencyManager ocrDependencyManager, IFormFactory formFactory)
        {
            this.options = options;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.errorOutput = errorOutput;
            this.emailer = emailer;
            this.scannedImageImporter = scannedImageImporter;
            this.userConfigManager = userConfigManager;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.imageSettingsContainer = imageSettingsContainer;
            this.operationFactory = operationFactory;
            this.appConfigManager = appConfigManager;
            this.ocrDependencyManager = ocrDependencyManager;
            this.formFactory = formFactory;
        }

        private void OutputVerbose(string value, params object[] args)
        {
            if (options.Verbose)
            {
                Console.WriteLine(value, args);
            }
        }

        public void Execute()
        {
            try
            {
                if (!ValidateOptions())
                {
                    return;
                }

                startTime = DateTime.Now;
                ConsoleOverwritePrompt.ForceOverwrite = options.ForceOverwrite;

                if (options.Install != null)
                {
                    InstallComponents();
                    if (options.OutputPath == null && options.EmailFileName == null && !options.AutoSave)
                    {
                        return;
                    }
                }

                if (!PreCheckOverwriteFile())
                {
                    return;
                }

                imageList = new ScannedImageList();

                if (options.ImportPath != null)
                {
                    ImportImages();
                }

                if (options.Number > 0)
                {
                    ScanProfile profile;
                    if (!GetProfile(out profile))
                    {
                        return;
                    }

                    PerformScan(profile);
                }

                ReorderScannedImages();

                if (options.OutputPath != null)
                {
                    ExportScannedImages();
                }

                if (options.EmailFileName != null)
                {
                    EmailScannedImages();
                }

                foreach (var image in imageList.Images)
                {
                    image.Dispose();
                }

                imageList = null;
            }
            catch (Exception ex)
            {
                Log.FatalException("An error occurred that caused the console application to close.", ex);
                Console.WriteLine(ConsoleResources.UnexpectedError);
            }
            finally
            {
                if (options.WaitForEnter)
                {
                    Console.ReadLine();
                }
            }
        }

        private void InstallComponents()
        {
            var availableComponents = new List<(DownloadInfo download, ExternalComponent component)>();
            if (ocrDependencyManager.Components.Tesseract304.IsSupported)
            {
                availableComponents.Add((ocrDependencyManager.Downloads.Tesseract304, ocrDependencyManager.Components.Tesseract304));
            } else if (ocrDependencyManager.Components.Tesseract304Xp.IsSupported)
            {
                availableComponents.Add((ocrDependencyManager.Downloads.Tesseract304Xp, ocrDependencyManager.Components.Tesseract304Xp));
            }
            foreach (var lang in ocrDependencyManager.Languages.Keys)
            {
                availableComponents.Add((ocrDependencyManager.Downloads.Tesseract304Languages[lang], ocrDependencyManager.Components.Tesseract304Languages[lang]));
            }
            availableComponents.Add((GhostscriptPdfRenderer.Dependencies.GhostscriptDownload, GhostscriptPdfRenderer.Dependencies.GhostscriptComponent));

            var componentDict = availableComponents.ToDictionary(x => x.component.Id.ToLowerInvariant());
            var installId = options.Install.ToLowerInvariant();
            if (!componentDict.TryGetValue(installId, out var toInstall))
            {
                Console.WriteLine(ConsoleResources.ComponentNotAvailable);
                return;
            }
            if (toInstall.component.IsInstalled)
            {
                Console.WriteLine(ConsoleResources.ComponentAlreadyInstalled);
                return;
            }
            // Using a form here is not ideal (since this is supposed to be a console app), but good enough for now
            // Especially considering wia/twain often show forms anyway
            var progressForm = formFactory.Create<FDownloadProgress>();
            if (toInstall.component.Id.StartsWith("ocr-") && componentDict.TryGetValue("ocr", out var ocrExe) && !ocrExe.component.IsInstalled)
            {
                progressForm.QueueFile(ocrExe.download, ocrExe.component.Install);
                if (options.Verbose)
                {
                    Console.WriteLine(ConsoleResources.Installing, ocrExe.component.Id);
                }
            }
            progressForm.QueueFile(toInstall.download, toInstall.component.Install);
            if (options.Verbose)
            {
                Console.WriteLine(ConsoleResources.Installing, toInstall.component.Id);
            }
            progressForm.ShowDialog();
        }
        
        private void ReorderScannedImages()
        {
            var e = new List<int>();

            if (options.AltDeinterleave)
            {
                imageList.AltDeinterleave(e);
            }
            else if (options.Deinterleave)
            {
                imageList.Deinterleave(e);
            }
            else if (options.AltInterleave)
            {
                imageList.AltInterleave(e);
            }
            else if (options.Interleave)
            {
                imageList.Interleave(e);
            }

            if (options.Reverse)
            {
                imageList.Reverse(e);
            }
        }

        private bool PreCheckOverwriteFile()
        {
            if (options.OutputPath == null)
            {
                // Email, so no check needed
                return true;
            }
            var subPath = fileNamePlaceholders.SubstitutePlaceholders(options.OutputPath, startTime);
            if (IsPdfFile(subPath)
                && File.Exists(subPath)
                && !options.ForceOverwrite)
            {
                errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, Path.GetFullPath(subPath)));
                return false;
            }
            return true;
        }

        private void ImportImages()
        {
            OutputVerbose(ConsoleResources.Importing);

            ConsolePdfPasswordProvider.PasswordToProvide = options.ImportPassword;

            var filePaths = options.ImportPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var filePath in filePaths)
            {
                i++;
                try
                {
                    var images = scannedImageImporter.Import(filePath, (j, k) => true);
                    imageList.Images.AddRange(images);
                }
                catch (Exception ex)
                {
                    Log.ErrorException(string.Format(ConsoleResources.ErrorImporting, filePath), ex);
                    errorOutput.DisplayError(string.Format(ConsoleResources.ErrorImporting, filePath));
                    continue;
                }
                OutputVerbose(ConsoleResources.ImportedFile, i, filePaths.Length);
            }
        }

        private void EmailScannedImages()
        {
            if (imageList.Images.Count == 0)
            {
                errorOutput.DisplayError(ConsoleResources.NoPagesToEmail);
                return;
            }


            OutputVerbose(ConsoleResources.Emailing);

            var message = new EmailMessage
            {
                Subject = fileNamePlaceholders.SubstitutePlaceholders(options.EmailSubject, startTime, false) ?? "",
                BodyText = fileNamePlaceholders.SubstitutePlaceholders(options.EmailBody, startTime, false),
                AutoSend = options.EmailAutoSend,
                SilentSend = options.EmailSilentSend
            };

            message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.To, options.EmailTo));
            message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.Cc, options.EmailCc));
            message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.Bcc, options.EmailBcc));

            var tempFolder = new DirectoryInfo(Path.Combine(Paths.Temp, Path.GetRandomFileName()));
            tempFolder.Create();
            try
            {
                string attachmentName = fileNamePlaceholders.SubstitutePlaceholders(options.EmailFileName, startTime, false);
                string targetPath = Path.Combine(tempFolder.FullName, attachmentName);
                if (IsPdfFile(targetPath))
                {
                    if (options.OutputPath != null && IsPdfFile(options.OutputPath))
                    {
                        // The scan has already been exported to PDF, so use that file
                        OutputVerbose(ConsoleResources.AttachingExportedPDF, attachmentName);
                        message.Attachments.Add(new EmailAttachment
                        {
                            FilePath = actualOutputPath,
                            AttachmentName = attachmentName
                        });
                    }
                    else
                    {
                        // The scan hasn't bee exported to PDF yet, so it needs to be exported to the temp folder
                        OutputVerbose(ConsoleResources.ExportingPDFToAttach);
                        if (!DoExportToPdf(targetPath, true))
                        {
                            OutputVerbose(ConsoleResources.EmailNotSent);
                            return;
                        }
                        // Attach the PDF file
                        AttachFilesInFolder(tempFolder, message);
                    }
                }
                else
                {
                    // Export the images to the temp folder
                    // Don't bother to re-use previously exported images, because the possible different formats and multiple files makes it non-trivial,
                    // and exporting is pretty cheap anyway
                    OutputVerbose(ConsoleResources.ExportingImagesToAttach);
                    DoExportToImageFiles(targetPath);
                    // Attach the image file(s)
                    AttachFilesInFolder(tempFolder, message);
                }

                OutputVerbose(ConsoleResources.SendingEmail);
                if (emailer.SendEmail(message))
                {
                    OutputVerbose(ConsoleResources.EmailSent);
                }
                else
                {
                    OutputVerbose(ConsoleResources.EmailNotSent);
                }
            }
            finally
            {
                tempFolder.Delete(true);
            }
        }

        private void AttachFilesInFolder(DirectoryInfo folder, EmailMessage message)
        {
            foreach (var file in folder.EnumerateFiles())
            {
                OutputVerbose(ConsoleResources.Attaching, file.Name);
                message.Attachments.Add(new EmailAttachment
                {
                    FilePath = file.FullName,
                    AttachmentName = file.Name
                });
            }
        }

        public bool ValidateOptions()
        {
            // Most validation is done by the CommandLineParser library, but some constraints that can't be represented by that API need to be checked here
            if (options.OutputPath == null && options.EmailFileName == null && options.Install == null && !options.AutoSave)
            {
                errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequired);
                return false;
            }
            if (options.OutputPath == null && options.EmailFileName == null && options.ImportPath != null)
            {
                errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequiredForImport);
                return false;
            }
            return true;
        }

        private void ExportScannedImages()
        {
            if (imageList.Images.Count == 0)
            {
                errorOutput.DisplayError(ConsoleResources.NoPagesToExport);
                return;
            }

            OutputVerbose(ConsoleResources.Exporting);

            if (IsPdfFile(options.OutputPath))
            {
                ExportToPdf();
            }
            else
            {
                ExportToImageFiles();
            }
        }

        private bool IsPdfFile(string path)
        {
            string extension = Path.GetExtension(path);
            Debug.Assert(extension != null);
            return extension.ToLower() == ".pdf";
        }

        private void ExportToImageFiles()
        {
            var path = fileNamePlaceholders.SubstitutePlaceholders(options.OutputPath, startTime);
            DoExportToImageFiles(options.OutputPath);
            OutputVerbose(ConsoleResources.FinishedSavingImages, Path.GetFullPath(path));
        }

        private void DoExportToImageFiles(string outputPath)
        {
            // TODO: If I add new image settings this may break things
            imageSettingsContainer.ImageSettings = new ImageSettings { JpegQuality = options.JpegQuality };
            var op = operationFactory.Create<SaveImagesOperation>();
            int i = -1;
            op.StatusChanged += (sender, args) =>
            {
                if (op.Status.CurrentProgress > i)
                {
                    OutputVerbose(ConsoleResources.ExportingImage, op.Status.CurrentProgress + 1, imageList.Images.Count);
                    i = op.Status.CurrentProgress;
                }
            };
            op.Start(outputPath, startTime, imageList.Images);
            op.WaitUntilFinished();
        }

        private void ExportToPdf()
        {
            // Get a local copy of the path just for output
            actualOutputPath = fileNamePlaceholders.SubstitutePlaceholders(options.OutputPath, startTime);
            if (DoExportToPdf(options.OutputPath, false))
            {
                OutputVerbose(ConsoleResources.SuccessfullySavedPdf, actualOutputPath);
            }
        }

        private bool DoExportToPdf(string path, bool email)
        {
            var metadata = options.UseSavedMetadata ? pdfSettingsContainer.PdfSettings.Metadata : new PdfMetadata();
            metadata.Creator = ConsoleResources.NAPS2;
            if (options.PdfTitle != null)
            {
                metadata.Title = options.PdfTitle;
            }
            if (options.PdfAuthor != null)
            {
                metadata.Author = options.PdfAuthor;
            }
            if (options.PdfSubject != null)
            {
                metadata.Subject = options.PdfSubject;
            }
            if (options.PdfKeywords != null)
            {
                metadata.Keywords = options.PdfKeywords;
            }

            var encryption = options.UseSavedEncryptConfig ? pdfSettingsContainer.PdfSettings.Encryption : new PdfEncryption();
            if (options.EncryptConfig != null)
            {
                try
                {
                    using (Stream configFileStream = File.OpenRead(options.EncryptConfig))
                    {
                        var serializer = new XmlSerializer(typeof(PdfEncryption));
                        encryption = (PdfEncryption)serializer.Deserialize(configFileStream);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorException(ConsoleResources.CouldntLoadEncryptionConfig, ex);
                    errorOutput.DisplayError(ConsoleResources.CouldntLoadEncryptionConfig);
                }
            }

            var pdfSettings = new PdfSettings { Metadata = metadata, Encryption = encryption };

            bool useOcr = !options.DisableOcr && (options.EnableOcr || options.OcrLang != null || userConfigManager.Config.EnableOcr || appConfigManager.Config.OcrState == OcrState.Enabled);
            string ocrLanguageCode = useOcr ? (options.OcrLang ?? ocrDependencyManager.DefaultLanguageCode) : null;

            var op = operationFactory.Create<SavePdfOperation>();
            int i = -1;
            op.StatusChanged += (sender, args) =>
            {
                if (op.Status.CurrentProgress > i)
                {
                    OutputVerbose(ConsoleResources.ExportingPage, op.Status.CurrentProgress + 1, imageList.Images.Count);
                    i = op.Status.CurrentProgress;
                }
            };
            op.Start(path, startTime, imageList.Images, pdfSettings, ocrLanguageCode, email);
            op.WaitUntilFinished();
            return op.Status.Success;
        }

        private void PerformScan(ScanProfile profile)
        {
            OutputVerbose(ConsoleResources.BeginningScan);

            bool autoSaveEnabled = !appConfigManager.Config.DisableAutoSave && profile.EnableAutoSave && profile.AutoSaveSettings != null;
            if (options.AutoSave && !autoSaveEnabled)
            {
                errorOutput.DisplayError(ConsoleResources.AutoSaveNotEnabled);
                if (options.OutputPath == null && options.EmailFileName == null)
                {
                    return;
                }
            }

            IWin32Window parentWindow = new Form { Visible = false };
            totalPagesScanned = 0;
            foreach (int i in Enumerable.Range(1, options.Number))
            {
                if (options.Delay > 0)
                {
                    OutputVerbose(ConsoleResources.Waiting, options.Delay);
                    Thread.Sleep(options.Delay);
                }
                OutputVerbose(ConsoleResources.StartingScan, i, options.Number);
                pagesScanned = 0;
                scanPerformer.PerformScan(profile, new ScanParams { NoUI = true, NoAutoSave = !options.AutoSave }, parentWindow, null, ReceiveScannedImage);
                OutputVerbose(ConsoleResources.PagesScanned, pagesScanned);
            }
        }

        private bool GetProfile(out ScanProfile profile)
        {
            try
            {
                if (options.ProfileName == null)
                {
                    // If no profile is specified, use the default (if there is one)
                    profile = profileManager.Profiles.Single(x => x.IsDefault);
                }
                else
                {
                    // Use the profile with the specified name (case-sensitive)
                    profile = profileManager.Profiles.FirstOrDefault(x => x.DisplayName == options.ProfileName);
                    if (profile == null)
                    {
                        // If none found, try case-insensitive
                        profile = profileManager.Profiles.First(x => x.DisplayName.ToLower() == options.ProfileName.ToLower());
                    }
                }
            }
            catch (InvalidOperationException)
            {
                errorOutput.DisplayError(ConsoleResources.ProfileUnavailableOrAmbiguous);
                profile = null;
                return false;
            }
            return true;
        }

        public void ReceiveScannedImage(ScannedImage scannedImage)
        {
            imageList.Images.Add(scannedImage);
            pagesScanned++;
            totalPagesScanned++;
            OutputVerbose(ConsoleResources.ScannedPage, totalPagesScanned);
        }
    }
}
