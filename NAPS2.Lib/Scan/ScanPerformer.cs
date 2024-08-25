using System.Runtime.CompilerServices;
using System.Threading;
using NAPS2.ImportExport;
using NAPS2.Ocr;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;
#if !MAC
using NAPS2.Scan.Internal.Wia;
using NAPS2.Wia;
#endif

namespace NAPS2.Scan;

internal class ScanPerformer : IScanPerformer
{
    public static Driver ParseDriver(string? value)
    {
        return value switch
        {
            DriverNames.WIA => Driver.Wia,
            DriverNames.SANE => Driver.Sane,
            DriverNames.TWAIN => Driver.Twain,
            DriverNames.ESCL => Driver.Escl,
            DriverNames.APPLE => Driver.Apple,
            _ => Driver.Default
        };
    }

    public static string SystemDefaultDriverName =>
        ScanOptionsValidator.SystemDefaultDriver.ToString().ToLowerInvariant();

    private readonly ScanningContext _scanningContext;
    private readonly IDevicePrompt _devicePrompt;
    private readonly Naps2Config _config;
    private readonly OperationProgress _operationProgress;
    private readonly AutoSaver _autoSaver;
    private readonly IProfileManager _profileManager;
    private readonly ErrorOutput _errorOutput;
    private readonly ScanOptionsValidator _scanOptionsValidator;
    private readonly IScanBridgeFactory _scanBridgeFactory;
    private readonly OcrOperationManager _ocrOperationManager;

    public ScanPerformer(IDevicePrompt devicePrompt, Naps2Config config, OperationProgress operationProgress,
        AutoSaver autoSaver, IProfileManager profileManager, ErrorOutput errorOutput,
        ScanOptionsValidator scanOptionsValidator, IScanBridgeFactory scanBridgeFactory,
        ScanningContext scanningContext, OcrOperationManager ocrOperationManager)
    {
        _devicePrompt = devicePrompt;
        _config = config;
        _operationProgress = operationProgress;
        _autoSaver = autoSaver;
        _profileManager = profileManager;
        _errorOutput = errorOutput;
        _scanOptionsValidator = scanOptionsValidator;
        _scanBridgeFactory = scanBridgeFactory;
        _scanningContext = scanningContext;
        _ocrOperationManager = ocrOperationManager;
    }

    public async Task<DeviceChoice> PromptForDevice(ScanProfile scanProfile, bool allowAlwaysAsk, IntPtr dialogParent = default)
    {
        try
        {
            var options = BuildOptions(scanProfile, new ScanParams(), dialogParent);
            return await _devicePrompt.PromptForDevice(options, allowAlwaysAsk);
        }
        catch (Exception error)
        {
            HandleError(error);
            return DeviceChoice.None;
        }
    }

    public IAsyncEnumerable<ScanDevice> GetDevices(ScanProfile scanProfile, CancellationToken cancelToken = default)
    {
        var options = BuildOptions(scanProfile, new ScanParams(), IntPtr.Zero);
        var controller = CreateScanController(new ScanParams());
        return controller.GetDevices(options, cancelToken);
    }

    public async Task<ScanCaps> GetCaps(ScanProfile scanProfile, CancellationToken cancelToken = default)
    {
        var options = BuildOptions(scanProfile, new ScanParams(), IntPtr.Zero);
        options.Device = scanProfile.Device?.ToScanDevice(options.Driver);
        var controller = CreateScanController(new ScanParams());
        return await controller.GetCaps(options, cancelToken);
    }

    public async IAsyncEnumerable<ProcessedImage> PerformScan(ScanProfile scanProfile, ScanParams scanParams,
        IntPtr dialogParent = default, [EnumeratorCancellation] CancellationToken cancelToken = default)
    {
        var options = BuildOptions(scanProfile, scanParams, dialogParent);
        // Make sure we get a real driver value (not just "Default")
        options = _scanOptionsValidator.ValidateAll(options, _scanningContext, false);

        if (!await PopulateDevice(scanProfile, options))
        {
            // User cancelled out of a dialog
            yield break;
        }

        var controller = CreateScanController(scanParams);
        var op = new ScanOperation(options);

        controller.PageStart += (sender, args) => op.NextPage(args.PageNumber);
        controller.ScanEnd += (sender, args) =>
        {
            // Close the progress window before showing the error dialog
            op.Completed();
            if (args.Error != null)
            {
                HandleError(args.Error);
            }
        };
        controller.PropagateErrors = false;
        TranslateProgress(controller, op);

        ShowOperation(op, options, scanParams);
        cancelToken.Register(op.Cancel);

        var images = controller.Scan(options, op.CancelToken);

        if (scanProfile.EnableAutoSave && scanProfile.AutoSaveSettings != null && !scanParams.NoAutoSave)
        {
            images = _autoSaver.Save(scanProfile.AutoSaveSettings, images);
        }

        int pageCount = 0;
        try
        {
            await foreach (var image in images)
            {
                pageCount++;
                yield return image;
            }
        }
        finally
        {
            if (pageCount > 0)
            {
                // TODO: Test event logging
                Log.Event(EventType.Scan, new EventParams
                {
                    Name = MiscResources.Scan,
                    Pages = pageCount,
                    DeviceName = scanProfile.Device?.Name,
                    ProfileName = scanProfile.DisplayName,
                    BitDepth = scanProfile.BitDepth.Description()
                });
            }
        }
    }

    private ScanController CreateScanController(ScanParams scanParams)
    {
        var localPostProcessor = new LocalPostProcessor(_scanningContext, ConfigureOcrController(scanParams));
        var controller = new ScanController(_scanningContext, localPostProcessor, _scanOptionsValidator,
            _scanBridgeFactory);
        return controller;
    }

    private OcrController ConfigureOcrController(ScanParams scanParams)
    {
        OcrController ocrController = new OcrController(_scanningContext);
        if (scanParams.OcrParams?.LanguageCode != null)
        {
            scanParams.OcrCancelToken.Register(() => ocrController.CancelAll());
        }
        _ocrOperationManager.RegisterOcrController(ocrController);
        return ocrController;
    }

    private void HandleError(Exception error)
    {
        if (error is not ScanDriverException)
        {
            Log.ErrorException(error.Message, error);
            _errorOutput.DisplayError(error.Message, error);
        }
        else if (error is ScanDriverUnknownException)
        {
            Log.ErrorException(error.Message, error.InnerException!);
            _errorOutput.DisplayError(error.Message, error);
        }
        else if (error is not AlreadyHandledDriverException)
        {
            _errorOutput.DisplayError(error.Message);
        }
    }

    private void ShowOperation(ScanOperation op, ScanOptions scanOptions, ScanParams scanParams)
    {
        bool isWia10 = scanOptions.Driver == Driver.Wia && scanOptions.WiaOptions.WiaApiVersion == WiaApiVersion.Wia10;
        bool showingTwainProgress = scanOptions.Driver == Driver.Twain && scanOptions.TwainOptions.ShowProgress;
        if (scanParams.NoUI || scanOptions.UseNativeUI && !isWia10 || showingTwainProgress)
        {
            return;
        }

        Invoker.Current.InvokeDispatch(() =>
        {
            if (scanParams.Modal)
            {
                _operationProgress.ShowModalProgress(op);
            }
            else
            {
                _operationProgress.ShowBackgroundProgress(op);
            }
        });
    }

    private void TranslateProgress(ScanController controller, ScanOperation op)
    {
        var smoothProgress = new SmoothProgress();
        controller.PageStart += (_, _) => smoothProgress.Reset();
        controller.PageProgress += (_, args) => smoothProgress.InputProgressChanged(args.Progress);
        controller.ScanEnd += (_, _) => smoothProgress.Reset();
        smoothProgress.OutputProgressChanged +=
            (_, args) => op.Progress((int) Math.Round(args.Value * 1000), 1000);
    }

    private ScanOptions BuildOptions(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent)
    {
        var options = new ScanOptions
        {
            Driver = ParseDriver(scanProfile.DriverName),
            WiaOptions =
            {
                WiaApiVersion = scanProfile.WiaVersion,
                OffsetWidth = scanProfile.WiaOffsetWidth
            },
            TwainOptions =
            {
                Dsm = scanProfile.TwainImpl == TwainImpl.X64
                    ? TwainDsm.NewX64
                    : scanProfile.TwainImpl is TwainImpl.OldDsm or TwainImpl.Legacy
                        ? TwainDsm.Old
                        : TwainDsm.New,
                // MemXfer is the default
                TransferMode = scanProfile.TwainImpl is TwainImpl.Default or TwainImpl.MemXfer
                    ? TwainTransferMode.Memory
                    : TwainTransferMode.Native,
                ShowProgress = scanProfile.TwainProgress,
                IncludeWiaDevices = false
            },
            SaneOptions =
            {
                // We use a worker process for SANE so we should clean up after each operation
                KeepInitialized = false
            },
            EsclOptions =
            {
                SecurityPolicy = _config.Get(c => c.EsclSecurityPolicy)
            },
            KeyValueOptions = scanProfile.KeyValueOptions != null
                ? new KeyValueScanOptions(scanProfile.KeyValueOptions)
                : new KeyValueScanOptions(),
            ExcludeLocalIPs = true,
            BarcodeDetectionOptions =
            {
                DetectBarcodes = scanParams.DetectPatchT ||
                                 scanProfile.AutoSaveSettings?.Separator == SaveSeparator.PatchT,
                PatchTOnly = true
            },
            OcrParams = scanParams.OcrParams ?? OcrParams.Empty,
            Brightness = scanProfile.Brightness,
            Contrast = scanProfile.Contrast,
            Dpi = scanProfile.Resolution.Dpi,
            Quality = scanProfile.Quality,
            AutoDeskew = scanProfile.AutoDeskew,
            RotateDegrees = scanProfile.RotateDegrees,
            BitDepth = scanProfile.BitDepth.ToBitDepth(),
            DialogParent = dialogParent,
            MaxQuality = scanProfile.MaxQuality,
            PageAlign = scanProfile.PageAlign.ToHorizontalAlign(),
            PaperSource = scanProfile.PaperSource.ToPaperSource(),
            ScaleRatio = scanProfile.AfterScanScale.ToIntScaleFactor(),
            ThumbnailSize = scanParams.ThumbnailSize,
            ExcludeBlankPages = scanProfile.ExcludeBlankPages,
            FlipDuplexedPages = scanProfile.FlipDuplexedPages,
            BlankPageCoverageThreshold = scanProfile.BlankPageCoverageThreshold,
            BlankPageWhiteThreshold = scanProfile.BlankPageWhiteThreshold,
            BrightnessContrastAfterScan = scanProfile.BrightnessContrastAfterScan,
            CropToPageSize = scanProfile.ForcePageSizeCrop,
            StretchToPageSize = scanProfile.ForcePageSize,
            UseNativeUI = scanProfile.UseNativeUI,
            Device = null, // Set after
            PageSize = null, // Set after
        };

        var pageDimensions = scanProfile.PageSize.PageDimensions() ?? scanProfile.CustomPageSize;
        if (pageDimensions == null)
        {
            throw new ArgumentException("No page size specified");
        }

        options.PageSize =
            new PageSize(pageDimensions.Width, pageDimensions.Height, (PageSizeUnit) pageDimensions.Unit);

        return options;
    }

    private async Task<bool> PopulateDevice(ScanProfile scanProfile, ScanOptions options)
    {
        // If a device wasn't specified, prompt the user to pick one
        if (string.IsNullOrEmpty(scanProfile.Device?.ID))
        {
            options.Device = (await _devicePrompt.PromptForDevice(options, false)).Device;
            if (options.Device == null)
            {
                return false;
            }

            // Persist the device in the profile if configured to do so
            if (_config.Get(c => c.AlwaysRememberDevice))
            {
                scanProfile.Device = ScanProfileDevice.FromScanDevice(options.Device);
                _profileManager.Save();
            }
        }
        else
        {
            options.Device = scanProfile.Device?.ToScanDevice(options.Driver);
        }

        return true;
    }
}