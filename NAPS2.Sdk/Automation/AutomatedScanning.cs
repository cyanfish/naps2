using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NAPS2.Config;
using NAPS2.Config.Experimental;
using NAPS2.Dependencies;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Scan;
using NAPS2.Images;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Automation
{
    public class AutomatedScanning
    {
        private readonly IEmailProviderFactory emailProviderFactory;
        private readonly IScanPerformer scanPerformer;
        private readonly ErrorOutput errorOutput;
        private readonly IScannedImageImporter scannedImageImporter;
        private readonly ImageSettingsContainer imageSettingsContainer;
        private readonly IOperationFactory operationFactory;
        private readonly OcrEngineManager ocrEngineManager;
        private readonly IFormFactory formFactory;
        private readonly ConfigScopes configScopes;
        private readonly ConfigProvider<CommonConfig> configProvider;
        private readonly ConfigProvider<PdfSettings> pdfSettingsProvider;

        private readonly AutomatedScanningOptions options;
        private List<List<ScannedImage>> scanList;
        private int pagesScanned;
        private int totalPagesScanned;
        private Placeholders placeholders;
        private List<string> actualOutputPaths;
        private OcrParams ocrParams;

        public AutomatedScanning(AutomatedScanningOptions options, IScanPerformer scanPerformer, ErrorOutput errorOutput, IEmailProviderFactory emailProviderFactory, IScannedImageImporter scannedImageImporter, ImageSettingsContainer imageSettingsContainer, IOperationFactory operationFactory, OcrEngineManager ocrEngineManager, IFormFactory formFactory, ConfigScopes configScopes, ConfigProvider<CommonConfig> configProvider, ConfigProvider<PdfSettings> pdfSettingsProvider)
        {
            this.options = options;
            this.scanPerformer = scanPerformer;
            this.errorOutput = errorOutput;
            this.emailProviderFactory = emailProviderFactory;
            this.scannedImageImporter = scannedImageImporter;
            this.imageSettingsContainer = imageSettingsContainer;
            this.operationFactory = operationFactory;
            this.ocrEngineManager = ocrEngineManager;
            this.formFactory = formFactory;
            this.configScopes = configScopes;
            this.configProvider = configProvider;
            this.pdfSettingsProvider = pdfSettingsProvider;
        }

        public IEnumerable<ScannedImage> AllImages => scanList.SelectMany(x => x);

        private void OutputVerbose(string value, params object[] args)
        {
            if (options.Verbose)
            {
                Console.WriteLine(value, args);
            }
        }

        public async Task Execute()
        {
            try
            {
                if (!ValidateOptions())
                {
                    return;
                }

                placeholders = Placeholders.All.WithDate(DateTime.Now);
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

                scanList = new List<List<ScannedImage>>();

                if (options.ImportPath != null)
                {
                    await ImportImages();
                }

                ConfigureOcr();

                if (options.Number > 0)
                {
                    if (!GetProfile(out ScanProfile profile))
                    {
                        return;
                    }

                    await PerformScan(profile);
                }

                ReorderScannedImages();

                if (options.OutputPath != null)
                {
                    await ExportScannedImages();
                }

                if (options.EmailFileName != null)
                {
                    await EmailScannedImages();
                }

                foreach (var image in AllImages)
                {
                    image.Dispose();
                }
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

        private void ConfigureOcr()
        {
            bool canUseOcr = IsPdfFile(options.OutputPath) || IsPdfFile(options.EmailFileName);
            if (!canUseOcr)
            {
                return;
            }
            if (options.DisableOcr)
            {
                configScopes.Run.Set(c => c.EnableOcr = false);
            }
            else if (options.EnableOcr || options.OcrLang != null)
            {
                configScopes.Run.Set(c => c.EnableOcr = true);
            }
            configScopes.Run.Set(c => c.OcrLanguageCode = options.OcrLang);
            ocrParams = configProvider.DefaultOcrParams();
        }

        private void InstallComponents()
        {
            var availableComponents = new List<IExternalComponent>();
            var ocrEngine = ocrEngineManager.EngineToInstall;
            if (ocrEngine != null)
            {
                availableComponents.Add(ocrEngine.Component);
                availableComponents.AddRange(ocrEngine.LanguageComponents);
            }
            if (GhostscriptManager.IsSupported)
            {
                availableComponents.Add(GhostscriptManager.GhostscriptComponent);
            }

            var componentDict = availableComponents.ToDictionary(x => x.Id.ToLowerInvariant());
            var installId = options.Install.ToLowerInvariant();
            if (!componentDict.TryGetValue(installId, out var toInstall))
            {
                Console.WriteLine(ConsoleResources.ComponentNotAvailable);
                return;
            }
            if (toInstall.IsInstalled)
            {
                Console.WriteLine(ConsoleResources.ComponentAlreadyInstalled);
                return;
            }
            // Using a form here is not ideal (since this is supposed to be a console app), but good enough for now
            // Especially considering wia/twain often show forms anyway
            var progressForm = formFactory.Create<FDownloadProgress>();
            if (toInstall.Id.StartsWith("ocr-", StringComparison.InvariantCulture) && componentDict.TryGetValue("ocr", out var ocrExe) && !ocrExe.IsInstalled)
            {
                progressForm.QueueFile(ocrExe);
                if (options.Verbose)
                {
                    Console.WriteLine(ConsoleResources.Installing, ocrExe.Id);
                }
            }
            progressForm.QueueFile(toInstall);
            if (options.Verbose)
            {
                Console.WriteLine(ConsoleResources.Installing, toInstall.Id);
            }
            progressForm.ShowDialog();
        }

        private void ReorderScannedImages()
        {
            var sep = options.SplitPatchT ? SaveSeparator.PatchT
                : options.SplitScans ? SaveSeparator.FilePerScan
                    : options.SplitSize > 0 || options.Split ? SaveSeparator.FilePerPage
                        : SaveSeparator.None;
            scanList = SaveSeparatorHelper.SeparateScans(scanList, sep, options.SplitSize).Where(x => x.Count > 0).ToList();

            foreach (var scan in scanList)
            {
                var imageList = new ScannedImageList(scan);
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
        }

        private bool PreCheckOverwriteFile()
        {
            if (options.OutputPath == null)
            {
                // Email, so no check needed
                return true;
            }
            var subPath = placeholders.Substitute(options.OutputPath);
            if (IsPdfFile(subPath)
                && File.Exists(subPath)
                && !options.ForceOverwrite)
            {
                errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, Path.GetFullPath(subPath)));
                return false;
            }
            return true;
        }

        private async Task ImportImages()
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
                    var importParams = new ImportParams
                    {
                        Slice = Slice.Parse(filePath, out string actualPath),
                        DetectPatchCodes = options.SplitPatchT,
                        NoThumbnails = true
                    };
                    var images = await scannedImageImporter.Import(actualPath, importParams, (j, k) => { }, CancellationToken.None).ToList();
                    scanList.Add(images);
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

        private async Task EmailScannedImages()
        {
            if (scanList.Count == 0)
            {
                errorOutput.DisplayError(ConsoleResources.NoPagesToEmail);
                return;
            }


            OutputVerbose(ConsoleResources.Emailing);

            var message = new EmailMessage
            {
                Subject = placeholders.Substitute(options.EmailSubject, false) ?? "",
                BodyText = placeholders.Substitute(options.EmailBody, false),
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
                string targetPath = Path.Combine(tempFolder.FullName, options.EmailFileName);
                if (IsPdfFile(targetPath))
                {
                    if (options.OutputPath != null && IsPdfFile(options.OutputPath))
                    {
                        // The scan has already been exported to PDF, so use that file
                        OutputVerbose(ConsoleResources.AttachingExportedPDF);
                        int digits = (int)Math.Floor(Math.Log10(scanList.Count)) + 1;
                        int i = 0;
                        foreach (var path in actualOutputPaths)
                        {
                            string attachmentName = placeholders.Substitute(options.EmailFileName, false, i++, scanList.Count > 1 ? digits : 0);
                            message.Attachments.Add(new EmailAttachment
                            {
                                FilePath = path,
                                AttachmentName = attachmentName
                            });
                        }
                    }
                    else
                    {
                        // The scan hasn't bee exported to PDF yet, so it needs to be exported to the temp folder
                        OutputVerbose(ConsoleResources.ExportingPDFToAttach);
                        if (!await DoExportToPdf(targetPath, true))
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
                    await DoExportToImageFiles(targetPath);
                    // Attach the image file(s)
                    AttachFilesInFolder(tempFolder, message);
                }

                OutputVerbose(ConsoleResources.SendingEmail);
                if (await emailProviderFactory.Default.SendEmail(message, (j, k) => { }, CancellationToken.None))
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

        private async Task ExportScannedImages()
        {
            if (scanList.Count == 0)
            {
                errorOutput.DisplayError(ConsoleResources.NoPagesToExport);
                return;
            }

            OutputVerbose(ConsoleResources.Exporting);

            if (IsPdfFile(options.OutputPath))
            {
                await ExportToPdf();
            }
            else
            {
                await ExportToImageFiles();
            }
        }

        private bool IsPdfFile(string path)
        {
            if (path == null) return false;
            string extension = Path.GetExtension(path);
            Debug.Assert(extension != null);
            return extension.ToLower() == ".pdf";
        }

        private async Task ExportToImageFiles()
        {
            var path = placeholders.Substitute(options.OutputPath);
            await DoExportToImageFiles(options.OutputPath);
            OutputVerbose(ConsoleResources.FinishedSavingImages, Path.GetFullPath(path));
        }

        private async Task DoExportToImageFiles(string outputPath)
        {
            // TODO: If I add new image settings this may break things
            imageSettingsContainer.LocalImageSettings = new ImageSettings
            {
                JpegQuality = options.JpegQuality,
                TiffCompression = Enum.TryParse<TiffCompression>(options.TiffComp, true, out var tc) ? tc : TiffCompression.Auto
            };

            foreach (var scan in scanList)
            {
                var op = operationFactory.Create<SaveImagesOperation>();
                int i = -1;
                op.StatusChanged += (sender, args) =>
                {
                    if (op.Status.CurrentProgress > i)
                    {
                        OutputVerbose(ConsoleResources.ExportingImage, op.Status.CurrentProgress + 1, scan.Count);
                        i = op.Status.CurrentProgress;
                    }
                };
                op.Start(outputPath, placeholders, scan);
                await op.Success;
            }
        }

        private async Task ExportToPdf()
        {
            await DoExportToPdf(options.OutputPath, false);
        }

        private async Task<bool> DoExportToPdf(string path, bool email)
        {
            var defaultVal = options.UseSavedMetadata ? null : "";
            configScopes.Run.SetAll(new CommonConfig
            {
                PdfSettings =
                {
                    Metadata =
                    {
                        Author = options.PdfAuthor ?? defaultVal,
                        Creator = ConsoleResources.NAPS2,
                        Keywords = options.PdfKeywords ?? defaultVal,
                        Subject = options.PdfSubject ?? defaultVal,
                        Title = options.PdfTitle ?? defaultVal
                    }
                }
            });
            
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

            var compat = PdfCompat.Default;
            if (options.PdfCompat != null)
            {
                var t = options.PdfCompat.Replace(" ", "").Replace("-", "");
                if (t.EndsWith("a1b", StringComparison.InvariantCultureIgnoreCase))
                {
                    compat = PdfCompat.PdfA1B;
                }
                else if (t.EndsWith("a2b", StringComparison.InvariantCultureIgnoreCase))
                {
                    compat = PdfCompat.PdfA2B;
                }
                else if (t.EndsWith("a3b", StringComparison.InvariantCultureIgnoreCase))
                {
                    compat = PdfCompat.PdfA3B;
                }
                else if (t.EndsWith("a3u", StringComparison.InvariantCultureIgnoreCase))
                {
                    compat = PdfCompat.PdfA3U;
                }
            }

            var pdfSettings = new PdfSettings { Metadata = metadata, Encryption = encryption, Compat = compat };

            int scanIndex = 0;
            actualOutputPaths = new List<string>();
            foreach (var fileContents in scanList)
            {
                var op = operationFactory.Create<SavePdfOperation>();
                int i = -1;
                op.StatusChanged += (sender, args) =>
                {
                    if (op.Status.CurrentProgress > i)
                    {
                        OutputVerbose(ConsoleResources.ExportingPage, op.Status.CurrentProgress + 1, fileContents.Count);
                        i = op.Status.CurrentProgress;
                    }
                };
                int digits = (int)Math.Floor(Math.Log10(scanList.Count)) + 1;
                string actualPath = placeholders.Substitute(path, true, scanIndex++, scanList.Count > 1 ? digits : 0);
                op.Start(actualPath, placeholders, fileContents, pdfSettingsProvider, new OcrContext(ocrParams), email, null);
                if (!await op.Success)
                {
                    return false;
                }
                actualOutputPaths.Add(actualPath);
                if (!email)
                {
                    OutputVerbose(ConsoleResources.SuccessfullySavedPdf, actualPath);
                }
            }
            return true;
        }

        private async Task PerformScan(ScanProfile profile)
        {
            OutputVerbose(ConsoleResources.BeginningScan);

            bool autoSaveEnabled = !configProvider.Get(c => c.DisableAutoSave) && profile.EnableAutoSave && profile.AutoSaveSettings != null;
            if (options.AutoSave && !autoSaveEnabled)
            {
                errorOutput.DisplayError(ConsoleResources.AutoSaveNotEnabled);
                if (options.OutputPath == null && options.EmailFileName == null)
                {
                    return;
                }
            }

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
                scanList.Add(new List<ScannedImage>());
                var scanParams = new ScanParams
                {
                    NoUI = !options.Progress,
                    NoAutoSave = !options.AutoSave,
                    NoThumbnails = true,
                    DetectPatchCodes = options.SplitPatchT,
                    DoOcr = ocrParams != null,
                    OcrParams = ocrParams
                };
                var source = scanPerformer.PerformScan(profile, scanParams);
                await source.ForEach(ReceiveScannedImage);
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
                    profile = ProfileManager.Current.Profiles.Single(x => x.IsDefault);
                }
                else
                {
                    // Use the profile with the specified name (try case-sensitive first, then case-insensitive)
                    profile = ProfileManager.Current.Profiles.FirstOrDefault(x => x.DisplayName == options.ProfileName) ??
                              ProfileManager.Current.Profiles.First(x => x.DisplayName.ToLower() == options.ProfileName.ToLower());
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
            scanList.Last().Add(scannedImage);
            pagesScanned++;
            totalPagesScanned++;
            OutputVerbose(ConsoleResources.ScannedPage, totalPagesScanned);
        }
    }
}
