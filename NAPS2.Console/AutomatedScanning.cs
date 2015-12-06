/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2015  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using NAPS2.Config;
using NAPS2.Console.Lang.Resources;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Console
{
    using Console = System.Console;

    public class AutomatedScanning
    {
        private readonly ImageSaver imageSaver;
        private readonly IEmailer emailer;
        private readonly IPdfExporter pdfExporter;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IErrorOutput errorOutput;
        private readonly IScannedImageImporter scannedImageImporter;
        private readonly IUserConfigManager userConfigManager;
        private readonly PdfSettingsContainer pdfSettingsContainer;
        private readonly FileNamePlaceholders fileNamePlaceholders;
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly PdfSaver pdfSaver;

        private readonly AutomatedScanningOptions options;
        private List<IScannedImage> scannedImages;
        private int pagesScanned;
        private int totalPagesScanned;
        private DateTime startTime;

        public AutomatedScanning(AutomatedScanningOptions options, ImageSaver imageSaver, IPdfExporter pdfExporter, IProfileManager profileManager, IScanPerformer scanPerformer, IErrorOutput errorOutput, IEmailer emailer, IScannedImageImporter scannedImageImporter, IUserConfigManager userConfigManager, PdfSettingsContainer pdfSettingsContainer, FileNamePlaceholders fileNamePlaceholders, ImageSettingsContainer imageSettingsContainer, PdfSaver pdfSaver)
        {
            this.options = options;
            this.imageSaver = imageSaver;
            this.pdfExporter = pdfExporter;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.errorOutput = errorOutput;
            this.emailer = emailer;
            this.scannedImageImporter = scannedImageImporter;
            this.userConfigManager = userConfigManager;
            this.pdfSettingsContainer = pdfSettingsContainer;
            this.fileNamePlaceholders = fileNamePlaceholders;
            this.imageSettingsContainer = imageSettingsContainer;
            this.pdfSaver = pdfSaver;
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
            if (!ValidateOptions())
            {
                return;
            }

            startTime = DateTime.Now;
            ConsoleOverwritePrompt.ForceOverwrite = options.ForceOverwrite;

            if (!PreCheckOverwriteFile())
            {
                return;
            }

            scannedImages = new List<IScannedImage>();

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

            if (options.OutputPath != null)
            {
                ExportScannedImages();
            }

            if (options.EmailFileName != null)
            {
                EmailScannedImages();
            }

            foreach (var image in scannedImages)
            {
                image.Dispose();
            }

            scannedImages = null;
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
                    var images = scannedImageImporter.Import(filePath);
                    scannedImages.AddRange(images);
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
            if (scannedImages.Count == 0)
            {
                errorOutput.DisplayError(ConsoleResources.NoPagesToEmail);
                return;
            }


            OutputVerbose(ConsoleResources.Emailing);

            var message = new EmailMessage
            {
                Subject = options.EmailSubject ?? "",
                BodyText = options.EmailBody,
                AutoSend = options.EmailAutoSend,
                SilentSend = options.EmailSilentSend
            };

            message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.To, options.EmailTo));
            message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.Cc, options.EmailCc));
            message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.Bcc, options.EmailTo));

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
                            FilePath = options.OutputPath,
                            AttachmentName = attachmentName
                        });
                    }
                    else
                    {
                        // The scan hasn't bee exported to PDF yet, so it needs to be exported to the temp folder
                        OutputVerbose(ConsoleResources.ExportingPDFToAttach);
                        DoExportToPdf(targetPath);
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
            if (options.OutputPath == null && options.EmailFileName == null)
            {
                errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequired);
                return false;
            }
            return true;
        }

        private void ExportScannedImages()
        {
            if (scannedImages.Count == 0)
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
            imageSaver.SaveImages(outputPath, startTime, scannedImages, i =>
            {
                OutputVerbose(ConsoleResources.ExportingImage, i, scannedImages.Count);
                return true;
            });
        }

        private void ExportToPdf()
        {
            // Get a local copy of the path just for output
            var path = fileNamePlaceholders.SubstitutePlaceholders(options.OutputPath, startTime);
            if (DoExportToPdf(options.OutputPath))
            {
                OutputVerbose(ConsoleResources.SuccessfullySavedPdf, path);
            }
        }

        private bool DoExportToPdf(string path)
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

            bool useOcr = !options.DisableOcr && (options.EnableOcr || options.OcrLang != null || userConfigManager.Config.EnableOcr);
            string ocrLanguageCode = useOcr ? (options.OcrLang ?? userConfigManager.Config.OcrLanguageCode) : null;

            return pdfSaver.SavePdf(path, startTime, scannedImages, pdfSettings, ocrLanguageCode, i =>
            {
                OutputVerbose(ConsoleResources.ExportingPage, i, scannedImages.Count);
                return true;
            });
        }

        private void PerformScan(ScanProfile profile)
        {
            OutputVerbose(ConsoleResources.BeginningScan);

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
                scanPerformer.PerformScan(profile, new ScanParams(), parentWindow, ReceiveScannedImage);
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

        public void ReceiveScannedImage(IScannedImage scannedImage)
        {
            scannedImages.Add(scannedImage);
            pagesScanned++;
            totalPagesScanned++;
            OutputVerbose(ConsoleResources.ScannedPage, totalPagesScanned);
        }
    }
}
