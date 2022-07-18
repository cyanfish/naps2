using System.Threading;
using NAPS2.Config.Model;
using NAPS2.Dependencies;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Ocr;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Serialization;
using NAPS2.WinForms;

namespace NAPS2.Automation;

public class AutomatedScanning
{
    private readonly ImageContext _imageContext;
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly IScanPerformer _scanPerformer;
    private readonly ErrorOutput _errorOutput;
    private readonly IScannedImageImporter _scannedImageImporter;
    private readonly IOperationFactory _operationFactory;
    private readonly TesseractLanguageManager _tesseractLanguageManager;
    private readonly IFormFactory _formFactory;
    private readonly Naps2Config _config;
    private readonly IProfileManager _profileManager;
    private readonly RecoveryStorageManager _recoveryStorageManager;
    private readonly ScanningContext _scanningContext;

    private readonly ConsoleOutput _output;
    private readonly AutomatedScanningOptions _options;
    private PdfEncryption _parsedEncryptConfigOption = null!;
    private List<List<ProcessedImage>> _scanList = null!;
    private int _pagesScanned;
    private int _totalPagesScanned;
    private Placeholders _placeholders = null!;
    private List<string> _actualOutputPaths = null!;
    private OcrParams _ocrParams = null!;

    public AutomatedScanning(ConsoleOutput output, AutomatedScanningOptions options, ImageContext imageContext,
        IScanPerformer scanPerformer, ErrorOutput errorOutput, IEmailProviderFactory emailProviderFactory,
        IScannedImageImporter scannedImageImporter, IOperationFactory operationFactory,
        TesseractLanguageManager tesseractLanguageManager, IFormFactory formFactory, Naps2Config config,
        IProfileManager profileManager, RecoveryStorageManager recoveryStorageManager, ScanningContext scanningContext)
    {
        _output = output;
        _options = options;
        _imageContext = imageContext;
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _emailProviderFactory = emailProviderFactory;
        _scannedImageImporter = scannedImageImporter;
        _operationFactory = operationFactory;
        _tesseractLanguageManager = tesseractLanguageManager;
        _formFactory = formFactory;
        _config = config;
        _profileManager = profileManager;
        _recoveryStorageManager = recoveryStorageManager;
        _scanningContext = scanningContext;
    }

    public IEnumerable<ProcessedImage> AllImages => _scanList.SelectMany(x => x);

    private void OutputVerbose(string value, params object[] args)
    {
        if (_options.Verbose)
        {
            _output.Writer.WriteLine(value, args);
        }
    }

    public async Task Execute()
    {
        bool hasUnexpectedException = false;
        try
        {
            if (!ValidateOptions())
            {
                return;
            }

            _placeholders = Placeholders.All.WithDate(DateTime.Now);

            if (_options.Install != null)
            {
                InstallComponents();
                if (_options.OutputPath == null && _options.EmailFileName == null && !_options.AutoSave)
                {
                    return;
                }
            }

            if (!PreCheckOverwriteFile())
            {
                return;
            }

            _scanList = new List<List<ProcessedImage>>();

            if (_options.ImportPath != null)
            {
                await ImportImages();
            }

            ConfigureOcr();

            if (_options.Number > 0)
            {
                if (!GetProfile(out ScanProfile profile))
                {
                    return;
                }

                await PerformScan(profile);
            }

            ReorderScannedImages();

            if (_options.OutputPath != null)
            {
                await ExportScannedImages();
            }

            if (_options.EmailFileName != null)
            {
                await EmailScannedImages();
            }
        }
        catch (Exception ex)
        {
            hasUnexpectedException = true;
            Log.FatalException("An error occurred that caused the console application to close.", ex);
            _output.Writer.WriteLine(ConsoleResources.UnexpectedError);
        }
        finally
        {
            if (!hasUnexpectedException)
            {
                _scanningContext.Dispose();
                _recoveryStorageManager.Dispose();
            }
            if (_options.WaitForEnter)
            {
                Console.ReadLine();
            }
        }
    }

    private void ConfigureOcr()
    {
        bool canUseOcr = IsPdfFile(_options.OutputPath) || IsPdfFile(_options.EmailFileName);
        if (!canUseOcr)
        {
            return;
        }
        if (_options.DisableOcr)
        {
            _config.Run.Set(c => c.EnableOcr, false);
        }
        else if (_options.EnableOcr || !string.IsNullOrEmpty(_options.OcrLang))
        {
            _config.Run.Set(c => c.EnableOcr, true);
        }
        _config.Run.Set(c => c.OcrLanguageCode, _options.OcrLang);
        _ocrParams = _config.DefaultOcrParams();
    }

    private void InstallComponents()
    {
        var availableComponents = new List<IExternalComponent>();
        availableComponents.AddRange(_tesseractLanguageManager.LanguageComponents);

        var componentDict = availableComponents.ToDictionary(x => x.Id.ToLowerInvariant());
        var installId = _options.Install!.ToLowerInvariant();
        if (!componentDict.TryGetValue(installId, out var toInstall))
        {
            _output.Writer.WriteLine(ConsoleResources.ComponentNotAvailable);
            return;
        }
        if (toInstall.IsInstalled)
        {
            _output.Writer.WriteLine(ConsoleResources.ComponentAlreadyInstalled);
            return;
        }
        // Using a form here is not ideal (since this is supposed to be a console app), but good enough for now
        // Especially considering wia/twain often show forms anyway
        var progressForm = _formFactory.Create<FDownloadProgress>();
        if (toInstall.Id.StartsWith("ocr-", StringComparison.InvariantCulture) &&
            componentDict.TryGetValue("ocr", out var ocrExe) && !ocrExe.IsInstalled)
        {
            progressForm.QueueFile(ocrExe);
            if (_options.Verbose)
            {
                _output.Writer.WriteLine(ConsoleResources.Installing, ocrExe.Id);
            }
        }
        progressForm.QueueFile(toInstall);
        if (_options.Verbose)
        {
            _output.Writer.WriteLine(ConsoleResources.Installing, toInstall.Id);
        }
        progressForm.ShowDialog();
    }

    private void ReorderScannedImages()
    {
        var sep = _options.SplitPatchT ? SaveSeparator.PatchT
            : _options.SplitScans ? SaveSeparator.FilePerScan
            : _options.SplitSize > 0 || _options.Split ? SaveSeparator.FilePerPage
            : SaveSeparator.None;
        _scanList = SaveSeparatorHelper.SeparateScans(_scanList, sep, _options.SplitSize).ToList();

        foreach (var scan in _scanList)
        {
            // To take advantage of the existing mutation logic we wrap the scan in a UiImageList then copy it back
            var imageList = new UiImageList(scan.Select(x => new UiImage(x)).ToList());

            if (_options.AltDeinterleave)
            {
                imageList.Mutate(new ImageListMutation.AltDeinterleave());
            }
            else if (_options.Deinterleave)
            {
                imageList.Mutate(new ImageListMutation.Deinterleave());
            }
            else if (_options.AltInterleave)
            {
                imageList.Mutate(new ImageListMutation.AltInterleave());
            }
            else if (_options.Interleave)
            {
                imageList.Mutate(new ImageListMutation.Interleave());
            }

            if (_options.Reverse)
            {
                imageList.Mutate(new ImageListMutation.ReverseAll());
            }
            
            scan.Clear();
            scan.AddRange(imageList.Images.Select(x => x.GetImageWeakReference().ProcessedImage));
        }
    }

    private bool PreCheckOverwriteFile()
    {
        if (_options.OutputPath == null)
        {
            // Email, so no check needed
            return true;
        }
        var subPath = _placeholders.Substitute(_options.OutputPath);
        if (IsPdfFile(subPath)
            && File.Exists(subPath)
            && !_options.ForceOverwrite)
        {
            _errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, Path.GetFullPath(subPath)));
            return false;
        }
        return true;
    }

    private async Task ImportImages()
    {
        OutputVerbose(ConsoleResources.Importing);

        var filePaths = _options.ImportPath!.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        int i = 0;
        foreach (var filePath in filePaths)
        {
            i++;
            try
            {
                var importParams = new ImportParams
                {
                    Slice = Slice.Parse(filePath, out string actualPath),
                    BarcodeDetectionOptions =
                    {
                        DetectBarcodes = _options.SplitPatchT,
                        PatchTOnly = true
                    }
                };
                var images = await _scannedImageImporter
                    .Import(actualPath, importParams, (j, k) => { }, CancellationToken.None).ToList();
                _scanList.Add(images);
            }
            catch (Exception ex)
            {
                Log.ErrorException(string.Format(ConsoleResources.ErrorImporting, filePath), ex);
                _errorOutput.DisplayError(string.Format(ConsoleResources.ErrorImporting, filePath));
                // TODO: Should we really continue?
                continue;
            }
            OutputVerbose(ConsoleResources.ImportedFile, i, filePaths.Length);
        }
    }

    private async Task EmailScannedImages()
    {
        if (_scanList.Count == 0)
        {
            _errorOutput.DisplayError(ConsoleResources.NoPagesToEmail);
            return;
        }


        OutputVerbose(ConsoleResources.Emailing);

        var message = new EmailMessage
        {
            Subject = _placeholders.Substitute(_options.EmailSubject, false) ?? "",
            BodyText = _placeholders.Substitute(_options.EmailBody, false),
            AutoSend = _options.EmailAutoSend,
            SilentSend = _options.EmailSilentSend
        };

        message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.To, _options.EmailTo));
        message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.Cc, _options.EmailCc));
        message.Recipients.AddRange(EmailRecipient.FromText(EmailRecipientType.Bcc, _options.EmailBcc));

        var tempFolder = new DirectoryInfo(Path.Combine(Paths.Temp, Path.GetRandomFileName()));
        tempFolder.Create();
        try
        {
            string targetPath = Path.Combine(tempFolder.FullName, _options.EmailFileName!);
            if (IsPdfFile(targetPath))
            {
                if (_options.OutputPath != null && IsPdfFile(_options.OutputPath))
                {
                    // The scan has already been exported to PDF, so use that file
                    OutputVerbose(ConsoleResources.AttachingExportedPDF);
                    int digits = (int) Math.Floor(Math.Log10(_scanList.Count)) + 1;
                    int i = 0;
                    foreach (var path in _actualOutputPaths)
                    {
                        string attachmentName = _placeholders.Substitute(_options.EmailFileName!, false, i++,
                            _scanList.Count > 1 ? digits : 0);
                        message.Attachments.Add(new EmailAttachment(path, attachmentName));
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
            if (await _emailProviderFactory.Default.SendEmail(message, (j, k) => { }, CancellationToken.None))
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
            message.Attachments.Add(new EmailAttachment(file.FullName, file.Name));
        }
    }

    public bool ValidateOptions()
    {
        // Most validation is done by the CommandLineParser library, but some constraints that can't be represented by that API need to be checked here
        if (_options.OutputPath == null && _options.EmailFileName == null && _options.Install == null &&
            !_options.AutoSave)
        {
            _errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequired);
            return false;
        }
        if (_options.OutputPath == null && _options.EmailFileName == null && _options.ImportPath != null)
        {
            _errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequiredForImport);
            return false;
        }

        if (new[] { _options.Interleave, _options.Deinterleave, _options.AltInterleave, _options.AltDeinterleave }
                .Count(x => x) > 1)
        {
            _errorOutput.DisplayError(ConsoleResources.OnlyOneInterleaveOption);
            return false;
        }

        if (!string.IsNullOrEmpty(_options.EncryptConfig))
        {
            try
            {
                var serializer = new XmlSerializer<PdfEncryption>();
                _parsedEncryptConfigOption = serializer.DeserializeFromFile(_options.EncryptConfig!);
            }
            catch (Exception ex)
            {
                _errorOutput.DisplayError(ConsoleResources.CouldntLoadEncryptionConfig, ex);
                return false;
            }
        }

        return true;
    }

    private async Task ExportScannedImages()
    {
        if (_scanList.Count == 0)
        {
            _errorOutput.DisplayError(ConsoleResources.NoPagesToExport);
            return;
        }

        OutputVerbose(ConsoleResources.Exporting);

        if (IsPdfFile(_options.OutputPath))
        {
            await ExportToPdf();
        }
        else
        {
            await ExportToImageFiles();
        }
    }

    private bool IsPdfFile(string? path)
    {
        if (path == null) return false;
        return Path.GetExtension(path)?.ToLower() == ".pdf";
    }

    private async Task ExportToImageFiles()
    {
        var path = _placeholders.Substitute(_options.OutputPath!);
        await DoExportToImageFiles(_options.OutputPath!);
        OutputVerbose(ConsoleResources.FinishedSavingImages, Path.GetFullPath(path));
    }

    private async Task DoExportToImageFiles(string outputPath)
    {
        _config.Run.Set(c => c.ImageSettings, new ImageSettings
        {
            JpegQuality = _options.JpegQuality,
            TiffCompression = Enum.TryParse<TiffCompression>(_options.TiffComp, true, out var tc)
                ? tc
                : TiffCompression.Auto
        });

        foreach (var scan in _scanList)
        {
            var op = _operationFactory.Create<SaveImagesOperation>();
            int i = -1;
            op.StatusChanged += (sender, args) =>
            {
                if (op.Status.CurrentProgress > i)
                {
                    OutputVerbose(ConsoleResources.ExportingImage, op.Status.CurrentProgress + 1, scan.Count);
                    i = op.Status.CurrentProgress;
                }
            };
            op.Start(outputPath, _placeholders, scan, _config.Get(c => c.ImageSettings));
            await op.Success;
        }
    }

    private async Task ExportToPdf()
    {
        await DoExportToPdf(_options.OutputPath!, false);
    }

    private async Task<bool> DoExportToPdf(string path, bool email)
    {
        var defaults = InternalDefaults.GetCommonConfig();
        
        if (!_options.UseSavedMetadata)
        {
            _config.Run.Set(c => c.PdfSettings.Metadata, defaults.PdfSettings.Metadata);
        }
        if (_options.PdfTitle != null)
        {
            _config.Run.Set(c => c.PdfSettings.Metadata.Title, _options.PdfTitle);
        }
        if (_options.PdfAuthor != null)
        {
            _config.Run.Set(c => c.PdfSettings.Metadata.Author, _options.PdfAuthor);
        }
        if (_options.PdfSubject != null)
        {
            _config.Run.Set(c => c.PdfSettings.Metadata.Subject, _options.PdfSubject);
        }
        if (_options.PdfKeywords != null)
        {
            _config.Run.Set(c => c.PdfSettings.Metadata.Keywords, _options.PdfKeywords);
        }

        if (!_options.UseSavedEncryptConfig)
        {
            _config.Run.Set(c => c.PdfSettings.Encryption, defaults.PdfSettings.Encryption);
        }
        if (_options.EncryptConfig != null)
        {
            // The actual file reading/parsing was done in ValidateOptions in case it failed
            _config.Run.Set(c => c.PdfSettings.Encryption, _parsedEncryptConfigOption);
        }

        var compat = PdfCompat.Default;
        if (!string.IsNullOrEmpty(_options.PdfCompat))
        {
            var t = _options.PdfCompat!.Replace(" ", "").Replace("-", "");
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
        _config.Run.Set(c => c.PdfSettings.Compat, compat);

        int scanIndex = 0;
        _actualOutputPaths = new List<string>();
        foreach (var fileContents in _scanList)
        {
            var op = _operationFactory.Create<SavePdfOperation>();
            int i = -1;
            op.StatusChanged += (sender, args) =>
            {
                if (op.Status.CurrentProgress > i)
                {
                    OutputVerbose(ConsoleResources.ExportingPage, op.Status.CurrentProgress + 1, fileContents.Count);
                    i = op.Status.CurrentProgress;
                }
            };
            int digits = (int) Math.Floor(Math.Log10(_scanList.Count)) + 1;
            string actualPath = _placeholders.Substitute(path, true, scanIndex++, _scanList.Count > 1 ? digits : 0);
            op.Start(actualPath, _placeholders, fileContents, _config.Get(c => c.PdfSettings), _ocrParams, email, null);
            if (!await op.Success)
            {
                return false;
            }
            _actualOutputPaths.Add(actualPath);
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

        bool autoSaveEnabled = !_config.Get(c => c.DisableAutoSave) && profile.EnableAutoSave &&
                               profile.AutoSaveSettings != null;
        if (_options.AutoSave && !autoSaveEnabled)
        {
            _errorOutput.DisplayError(ConsoleResources.AutoSaveNotEnabled);
            if (_options.OutputPath == null && _options.EmailFileName == null)
            {
                return;
            }
        }

        _totalPagesScanned = 0;
        foreach (int i in Enumerable.Range(1, _options.Number))
        {
            if (_options.Delay > 0)
            {
                OutputVerbose(ConsoleResources.Waiting, _options.Delay);
                Thread.Sleep(_options.Delay);
            }
            OutputVerbose(ConsoleResources.StartingScan, i, _options.Number);
            _pagesScanned = 0;
            _scanList.Add(new List<ProcessedImage>());
            var scanParams = new ScanParams
            {
                NoUI = !_options.Progress,
                NoAutoSave = !_options.AutoSave || !autoSaveEnabled,
                DetectPatchT = _options.SplitPatchT,
                OcrParams = _ocrParams
            };
            var source = await _scanPerformer.PerformScan(profile, scanParams);
            await source.ForEach(ReceiveScannedImage);
            OutputVerbose(ConsoleResources.PagesScanned, _pagesScanned);
        }
    }

    private bool GetProfile(out ScanProfile profile)
    {
        try
        {
            if (_options.ProfileName == null)
            {
                // If no profile is specified, use the default (if there is one)
                profile = _profileManager.Profiles.Single(x => x.IsDefault);
            }
            else
            {
                // Use the profile with the specified name (try case-sensitive first, then case-insensitive)
                profile = _profileManager.Profiles.FirstOrDefault(x => x.DisplayName == _options.ProfileName) ??
                          _profileManager.Profiles.First(x =>
                              x.DisplayName.ToLower() == _options.ProfileName.ToLower());
            }
        }
        catch (InvalidOperationException)
        {
            _errorOutput.DisplayError(ConsoleResources.ProfileUnavailableOrAmbiguous);
            profile = null!;
            return false;
        }
        return true;
    }

    public void ReceiveScannedImage(ProcessedImage scannedImage)
    {
        _scanList.Last().Add(scannedImage);
        _pagesScanned++;
        _totalPagesScanned++;
        OutputVerbose(ConsoleResources.ScannedPage, _totalPagesScanned);
    }
}