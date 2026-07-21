using System.Threading;
using NAPS2.Dependencies;
using NAPS2.EtoForms;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Email;
using NAPS2.ImportExport.Images;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;
using NAPS2.Serialization;

namespace NAPS2.Automation;

internal class AutomatedScanning
{
    private readonly ImageContext _imageContext;
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly IScanPerformer _scanPerformer;
    private readonly ErrorOutput _errorOutput;
    private readonly FileImporter _fileImporter;
    private readonly IOperationFactory _operationFactory;
    private readonly TesseractLanguageManager _tesseractLanguageManager;
    private readonly IFormFactory _formFactory;
    private readonly Naps2Config _config;
    private readonly IProfileManager _profileManager;
    private readonly RecoveryStorageManager _recoveryStorageManager;
    private readonly ScanningContext _scanningContext;
    private readonly IScanBridgeFactory _scanBridgeFactory;

    private readonly ConsoleOutput _output;
    private readonly AutomatedScanningOptions _options;
    private readonly CancellationTokenSource _cts = new();
    private PdfEncryption _parsedEncryptConfigOption = null!;
    private List<List<ProcessedImage>> _scanList = null!;
    private int _pagesScanned;
    private int _totalPagesScanned;
    private Placeholders _placeholders = null!;
    private List<string> _actualOutputPaths = null!;
    private OcrParams _ocrParams = null!;
    private PageDimensions? _pageDimensions;

    public AutomatedScanning(ConsoleOutput output, AutomatedScanningOptions options, ImageContext imageContext,
        IScanPerformer scanPerformer, ErrorOutput errorOutput, IEmailProviderFactory emailProviderFactory,
        FileImporter fileImporter, IOperationFactory operationFactory,
        TesseractLanguageManager tesseractLanguageManager, IFormFactory formFactory, Naps2Config config,
        IProfileManager profileManager, RecoveryStorageManager recoveryStorageManager, ScanningContext scanningContext,
        IScanBridgeFactory scanBridgeFactory)
    {
        _output = output;
        _options = options;
        _imageContext = imageContext;
        _scanPerformer = scanPerformer;
        _errorOutput = errorOutput;
        _emailProviderFactory = emailProviderFactory;
        _fileImporter = fileImporter;
        _operationFactory = operationFactory;
        _tesseractLanguageManager = tesseractLanguageManager;
        _formFactory = formFactory;
        _config = config;
        _profileManager = profileManager;
        _recoveryStorageManager = recoveryStorageManager;
        _scanningContext = scanningContext;
        _scanBridgeFactory = scanBridgeFactory;
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
        Console.CancelKeyPress += (_, e) =>
        {
            if (!_cts.IsCancellationRequested)
            {
                Console.WriteLine(ConsoleResources.Cancelling);
                _cts.Cancel();
                e.Cancel = true;
            }
        };
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
                await InstallComponents();
                if (_options.OutputPath == null && _options.EmailFileName == null && !_options.AutoSave)
                {
                    return;
                }
            }

            if (_options.ListDevices)
            {
                await ListDevices();
                return;
            }

            if (!PreCheckOverwriteFile())
            {
                return;
            }

            _scanList = [];

            if (_options.ImportPath != null)
            {
                await ImportImages();
            }

            if (_cts.IsCancellationRequested) return;

            ConfigureOcr();

            if (_options.Number > 0)
            {
                if (!GetProfile(out ScanProfile profile))
                {
                    return;
                }
                if (!await SetProfileOverrides(profile))
                {
                    return;
                }
                await PerformScan(profile);
            }

            if (_cts.IsCancellationRequested) return;

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
        catch (ScanDriverException ex) when (ex is not ScanDriverUnknownException)
        {
            _output.Writer.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            hasUnexpectedException = true;
            Log.FatalException("An error occurred that caused the console application to close.", ex);
            _output.Writer.WriteLine(ConsoleResources.UnexpectedError);
            _output.Writer.WriteLine(ex.Message);
            if (ex.InnerException != null)
            {
                _output.Writer.WriteLine(ex.InnerException.Message);
            }
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
        if (_options.NoProfile)
        {
            // Don't use OCR-enabled settings from the GUI if --noprofile is set
            _config.Run.Set(c => c.EnableOcr, false);
        }
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
        if (!string.IsNullOrEmpty(_options.OcrLang))
        {
            _config.Run.Set(c => c.OcrLanguageCode, _options.OcrLang);
        }
        _ocrParams = _config.DefaultOcrParams();
    }

    private async Task InstallComponents()
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
        var downloadController = new DownloadController(_scanningContext);
        if (toInstall.Id.StartsWith("ocr-", StringComparison.InvariantCulture) &&
            componentDict.TryGetValue("ocr", out var ocrExe) && !ocrExe.IsInstalled)
        {
            downloadController.QueueFile(ocrExe);
            if (_options.Verbose)
            {
                _output.Writer.WriteLine(ConsoleResources.Installing, ocrExe.Id);
            }
        }
        downloadController.QueueFile(toInstall);
        if (_options.Verbose)
        {
            _output.Writer.WriteLine(ConsoleResources.Installing, toInstall.Id);
        }
        downloadController.DownloadError += (_, _) =>
        {
            _errorOutput.DisplayError(MiscResources.FilesCouldNotBeDownloaded);
        };
        await downloadController.StartDownloadsAsync();
    }

    private async Task ListDevices()
    {
        var profile = new ScanProfile
        {
            DriverName = ScanPerformer.SystemDefaultDriverName
        };
        await SetProfileOverrides(profile);

        await foreach (var device in _scanPerformer.GetDevices(profile, _cts.Token))
        {
            _output.Writer.WriteLine(device.Name);
        }
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
            var imageList = UiImageList.FromImages(scan.Select(x => new UiImage(x)).ToList());

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
            var scan = new List<ProcessedImage>();
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
                await foreach (var image in _fileImporter.Import(actualPath, importParams, _cts.Token))
                {
                    scan.Add(PostProcessImportedImage(image));
                }
                _scanList.Add(scan);
            }
            catch (Exception ex)
            {
                Log.ErrorException(string.Format(ConsoleResources.ErrorImporting, filePath), ex);
                _errorOutput.DisplayError(string.Format(ConsoleResources.ErrorImporting, filePath));
                // TODO: Should we keep these images?
                foreach (var image in scan)
                {
                    image.Dispose();
                }
                // TODO: Should we really continue?
                continue;
            }
            OutputVerbose(ConsoleResources.ImportedFile, i, filePaths.Length);
        }
    }

    private ProcessedImage PostProcessImportedImage(ProcessedImage image)
    {
        // We separate post-processing for scanned images (as part of the scanning pipeline) and imported images (here).
        // The reason is that post-processing affects OCR, which runs as part of the scanning pipeline.
        if (_options.RotateDegrees != null)
        {
            image = image.WithTransform(new RotationTransform(_options.RotateDegrees.Value), true);
        }
        if (_options.Deskew)
        {
            using var rendered = image.Render();
            image = image.WithTransform(Deskewer.GetDeskewTransform(rendered), true);
        }
        return image;
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

        // TODO: We may want to normalize this to use SavePdfOperation's email functionality
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
            if (await _emailProviderFactory.Default.SendEmail(message, _cts.Token))
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
        foreach (var file in folder.EnumerateFiles().OrderBy(x => x.Name))
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
        if (_options.OutputPath == null && _options.EmailFileName == null && _options.Install == null &&
            !_options.AutoSave && !_options.ListDevices)
        {
            _errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequired);
            return false;
        }
        if (_options.OutputPath == null && _options.EmailFileName == null && _options.ImportPath != null)
        {
            _errorOutput.DisplayError(ConsoleResources.OutputOrEmailRequiredForImport);
            return false;
        }

        if ((_options.Device != null || _options.ListDevices) && _options.Driver == null)
        {
            _errorOutput.DisplayError(string.Format(ConsoleResources.DriverRequired, SupportedDriversString));
            return false;
        }

        if (_options.Driver != null)
        {
            if (ScanPerformer.ParseDriver(_options.Driver) == Driver.Default)
            {
                _errorOutput.DisplayError(string.Format(ConsoleResources.InvalidDriver, SupportedDriversString));
                return false;
            }
        }

        if (_options.PageSize != null)
        {
            var pageSize = PageSize.Parse(_options.PageSize);
            if (pageSize == null)
            {
                _errorOutput.DisplayError(ConsoleResources.CouldntParsePageSize);
                return false;
            }
            _pageDimensions = new PageDimensions
            {
                Width = pageSize.Width,
                Height = pageSize.Height,
                Unit = (LocalizedPageSizeUnit) pageSize.Unit
            };
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

    private string SupportedDriversString => string.Join(", ",
        ScanOptionsValidator.SupportedDrivers.Select(x => x.ToString().ToLowerInvariant()));

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
            _cts.Token.Register(op.Cancel);
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

        _config.Run.Set(c => c.PdfSettings.SinglePagePdfs, false);
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
        _actualOutputPaths = [];
        foreach (var fileContents in _scanList)
        {
            var op = _operationFactory.Create<SavePdfOperation>();
            _cts.Token.Register(op.Cancel);
            int i = -1;
            op.StatusChanged += (sender, args) =>
            {
                if (op.Status.CurrentProgress > i && op.Status.CurrentProgress < op.Status.MaxProgress)
                {
                    OutputVerbose(ConsoleResources.ExportingPage, op.Status.CurrentProgress + 1, fileContents.Count);
                    i = op.Status.CurrentProgress;
                }
            };
            int digits = (int) Math.Floor(Math.Log10(_scanList.Count)) + 1;
            string actualPath = _placeholders.Substitute(path, true, scanIndex++, _scanList.Count > 1 ? digits : 0);
            op.Start(actualPath, _placeholders, fileContents, _config.Get(c => c.PdfSettings), _ocrParams);
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
            if (i > 1 || !_options.FirstNow)
            {
                if (_options.WaitScan)
                {
                    OutputVerbose(ConsoleResources.PressEnterToScan);
                    var input = Console.ReadLine();
                    if (input == null)
                    {
                        // Ctrl+D (EOF) detected - stop scanning and save
                        OutputVerbose(ConsoleResources.StoppingScan);
                        break;
                    }
                }
                if (_options.Delay > 0)
                {
                    OutputVerbose(ConsoleResources.Waiting, _options.Delay);
                    Thread.Sleep(_options.Delay);
                }
            }
            OutputVerbose(ConsoleResources.StartingScan, i, _options.Number);
            _pagesScanned = 0;
            _scanList.Add([]);
            var scanParams = new ScanParams
            {
                NoUI = !_options.Progress,
                NoAutoSave = !_options.AutoSave || !autoSaveEnabled,
                DetectPatchT = _options.SplitPatchT,
                OcrParams = _ocrParams
            };
            await foreach (var image in _scanPerformer.PerformScan(profile, scanParams, cancelToken: _cts.Token))
            {
                ReceiveScannedImage(image);
            }
            OutputVerbose(ConsoleResources.PagesScanned, _pagesScanned);
        }
    }

    private bool GetProfile(out ScanProfile profile)
    {
        try
        {
            if (_options.NoProfile)
            {
                profile = new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
            }
            else if (_options.ProfileName != null)
            {
                // Use the profile with the specified name (try case-sensitive first, then case-insensitive)
                profile = _profileManager.Profiles.FirstOrDefault(x => x.DisplayName == _options.ProfileName) ??
                          _profileManager.Profiles.First(x =>
                              x.DisplayName.ToLower() == _options.ProfileName.ToLower());
            }
            else
            {
                // If no profile is specified, use the default (if there is one)
                profile = _profileManager.Profiles.FirstOrDefault(x => x.IsDefault) ??
                          new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
            }
            // If we have any profile overrides we do not want to risk persisting changes back to the main profile.
            // It shouldn't happen anyway as we shouldn't call ProfileManager.Save, but this provides a level of
            // protection against that.
            profile = profile.Clone();
        }
        catch (InvalidOperationException)
        {
            _errorOutput.DisplayError(ConsoleResources.ProfileUnavailableOrAmbiguous);
            profile = null!;
            return false;
        }
        return true;
    }

    private async Task<bool> SetProfileOverrides(ScanProfile profile)
    {
        if (!string.IsNullOrEmpty(_options.Driver))
        {
            var driver = ScanPerformer.ParseDriver(_options.Driver);
            profile.DriverName = driver.ToString().ToLowerInvariant();
        }
        if (_options.Source != null)
        {
            profile.PaperSource = _options.Source.Value;
        }
        if (_pageDimensions != null)
        {
            profile.PageSize = ScanPageSize.Custom;
            profile.CustomPageSize = _pageDimensions;
        }
        if (_options.Dpi != null)
        {
            profile.Resolution.Dpi = _options.Dpi.Value;
        }
        if (_options.BitDepth != null)
        {
            profile.BitDepth = _options.BitDepth switch
            {
                ConsoleBitDepth.Color => ScanBitDepth.C24Bit,
                ConsoleBitDepth.Gray => ScanBitDepth.Grayscale,
                ConsoleBitDepth.Bw => ScanBitDepth.BlackWhite,
                _ => ScanBitDepth.C24Bit
            };
        }
        if (_options.Deskew)
        {
            profile.AutoDeskew = true;
        }
        if (_options.RotateDegrees != null)
        {
            profile.RotateDegrees = _options.RotateDegrees.Value;
        }

        if (!string.IsNullOrEmpty(_options.Device))
        {
            var cts = new CancellationTokenSource();
            bool foundDevice = false;
            await foreach (var device in _scanPerformer.GetDevices(profile, cts.Token))
            {
                if (device.Name.ContainsInvariantIgnoreCase(_options.Device!))
                {
                    cts.Cancel();
                    profile.Device = new ScanProfileDevice(device.ID, device.Name);
                    foundDevice = true;
                    break;
                }
            }
            if (!foundDevice)
            {
                _errorOutput.DisplayError(SdkResources.DeviceNotFound);
                return false;
            }
        }

        if (!_options.ListDevices && profile.Device == null)
        {
            _errorOutput.DisplayError(ConsoleResources.NoDeviceSpecified);
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