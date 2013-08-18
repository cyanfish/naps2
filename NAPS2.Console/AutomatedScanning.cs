/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Console.Lang.Resources;
using NAPS2.Email;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Console
{
    using Console = System.Console;

    public class AutomatedScanning : IScanReceiver
    {
        private readonly ImageSaver imageSaver;
        private readonly IEmailer emailer;
        private readonly IPdfExporter pdfExporter;
        private readonly IProfileManager profileManager;
        private readonly IScanPerformer scanPerformer;
        private readonly IErrorOutput errorOutput;

        private readonly AutomatedScanningOptions options;
        private List<IScannedImage> scannedImages;
        private int pagesScanned;

        public AutomatedScanning(AutomatedScanningOptions options, ImageSaver imageSaver, IPdfExporter pdfExporter, IProfileManager profileManager, IScanPerformer scanPerformer, IErrorOutput errorOutput, IEmailer emailer)
        {
            this.options = options;
            this.imageSaver = imageSaver;
            this.pdfExporter = pdfExporter;
            this.profileManager = profileManager;
            this.scanPerformer = scanPerformer;
            this.errorOutput = errorOutput;
            this.emailer = emailer;
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

            ExtendedScanSettings profile;
            if (!GetProfile(out profile))
            {
                return;
            }

            PerformScan(profile);

            if (options.OutputPath != null)
            {
                ExportScannedImages();
            }

            if (options.EmailFileName != null)
            {
                EmailScannedImages();
            }

            if (options.WaitForEnter)
            {
                Console.ReadLine();
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

            AddRecipients(message, options.EmailTo, EmailRecipientType.To);
            AddRecipients(message, options.EmailCc, EmailRecipientType.Cc);
            AddRecipients(message, options.EmailBcc, EmailRecipientType.Bcc);

            var tempFolder = new DirectoryInfo(Path.Combine(Paths.Temp, Path.GetRandomFileName()));
            tempFolder.Create();
            try
            {
                string targetPath = Path.Combine(tempFolder.FullName, options.EmailFileName);
                if (IsPdfFile(targetPath))
                {
                    if (options.OutputPath != null && IsPdfFile(options.OutputPath))
                    {
                        // The scan has already been exported to PDF, so use that file
                        OutputVerbose(ConsoleResources.AttachingExportedPDF, options.EmailFileName);
                        message.Attachments.Add(new EmailAttachment
                        {
                            FilePath = options.OutputPath,
                            AttachmentName = options.EmailFileName
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

        private void AddRecipients(EmailMessage message, string addresses, EmailRecipientType recipientType)
        {
            if (string.IsNullOrWhiteSpace(addresses))
            {
                return;
            }
            foreach (string address in addresses.Split(','))
            {
                message.Recipients.Add(new EmailRecipient
                {
                    Name = address.Trim(),
                    Address = address.Trim(),
                    Type = recipientType
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
            DoExportToImageFiles(options.OutputPath);

            OutputVerbose(ConsoleResources.FinishedSavingImages, options.OutputPath);
        }

        private void DoExportToImageFiles(string outputPath)
        {
            imageSaver.SaveImages(outputPath, scannedImages, path =>
            {
                NotifyOverwrite(path);
                return options.ForceOverwrite;
            });
        }

        private void NotifyOverwrite(string path)
        {
            if (!options.ForceOverwrite)
            {
                errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, path));
            }
            if (options.ForceOverwrite)
            {
                OutputVerbose(ConsoleResources.Overwriting, path);
            }
        }

        private void ExportToPdf()
        {
            if (File.Exists(options.OutputPath))
            {
                NotifyOverwrite(options.OutputPath);
                if (!options.ForceOverwrite)
                {
                    return;
                }
            }

            try
            {
                DoExportToPdf(options.OutputPath);

                OutputVerbose(ConsoleResources.SuccessfullySavedPdf, options.OutputPath);
            }
            catch (UnauthorizedAccessException)
            {
                errorOutput.DisplayError(ConsoleResources.DontHavePermission);
            }
        }

        private void DoExportToPdf(string outputPath)
        {
            var pdfInfo = new PdfInfo
            {
                Title = ConsoleResources.ScannedImage,
                Subject = ConsoleResources.ScannedImage,
                Author = ConsoleResources.NAPS2
            };
            pdfExporter.Export(outputPath, scannedImages.Select(x => (Image)x.GetImage()), pdfInfo, i =>
            {
                OutputVerbose(ConsoleResources.ExportedPage, i, scannedImages.Count);
                return true;
            });
        }

        private void PerformScan(ExtendedScanSettings profile)
        {
            OutputVerbose(ConsoleResources.BeginningScan);

            scannedImages = new List<IScannedImage>();
            IWin32Window parentWindow = new Form { Visible = false };
            foreach (int i in Enumerable.Range(1, options.Number))
            {
                if (options.Delay > 0)
                {
                    OutputVerbose(ConsoleResources.Waiting, options.Delay);
                    Thread.Sleep(options.Delay);
                }
                OutputVerbose(ConsoleResources.StartingScan, i, options.Number);
                pagesScanned = 0;
                scanPerformer.PerformScan(profile, parentWindow, this);
                OutputVerbose(ConsoleResources.PagesScanned, pagesScanned);
            }
        }

        private bool GetProfile(out ExtendedScanSettings profile)
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
        }
    }
}
