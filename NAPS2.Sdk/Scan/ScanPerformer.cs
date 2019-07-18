using System;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Operation;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan
{
    public class ScanPerformer : IScanPerformer
    {
        private readonly IFormFactory formFactory;
        private readonly ConfigProvider<CommonConfig> configProvider;
        private readonly OperationProgress operationProgress;
        private readonly AutoSaver autoSaver;
        private readonly IProfileManager profileManager;
        private readonly ErrorOutput errorOutput;

        public ScanPerformer(IFormFactory formFactory, ConfigProvider<CommonConfig> configProvider, OperationProgress operationProgress, AutoSaver autoSaver,
            IProfileManager profileManager, ErrorOutput errorOutput)
        {
            this.formFactory = formFactory;
            this.configProvider = configProvider;
            this.operationProgress = operationProgress;
            this.autoSaver = autoSaver;
            this.profileManager = profileManager;
            this.errorOutput = errorOutput;
        }

        public async Task<ScanDevice> PromptForDevice(ScanProfile scanProfile, IntPtr dialogParent = default)
        {
            var options = BuildOptions(scanProfile, new ScanParams(), dialogParent);
            return await PromptForDevice(options);
        }

        public async Task<ScannedImageSource> PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default,
            CancellationToken cancelToken = default)
        {
            var options = BuildOptions(scanProfile, scanParams, dialogParent);
            if (!await PopulateDevice(scanProfile, options))
            {
                // User cancelled out of a dialog
                return ScannedImageSource.Empty;
            }

            var controller = new ScanController();
            // TODO: Consider how to handle operations with Twain (right now there are duplicate progress windows).
            var op = new ScanOperation(options.Device, options.PaperSource);

            controller.PageStart += (sender, args) => op.NextPage(args.PageNumber);
            controller.ScanEnd += (sender, args) => op.Completed();
            controller.ScanError += (sender, args) => HandleError(args.Exception);
            TranslateProgress(controller, op);

            ShowOperation(op, scanParams);
            cancelToken.Register(op.Cancel);

            var source = controller.Scan(options, op.CancelToken);

            if (scanProfile.EnableAutoSave && scanProfile.AutoSaveSettings != null)
            {
                source = autoSaver.Save(scanProfile.AutoSaveSettings, source);
            }
            
            var sink = new ScannedImageSink();
            source.ForEach(img => sink.PutImage(img)).ContinueWith(t =>
            {
                // Errors are handled by the ScanError callback so we ignore them here
                if (sink.ImageCount > 0)
                {
                    Log.Event(EventType.Scan, new EventParams
                    {
                        Name = MiscResources.Scan,
                        Pages = sink.ImageCount,
                        DeviceName = scanProfile.Device?.Name,
                        ProfileName = scanProfile.DisplayName,
                        BitDepth = scanProfile.BitDepth.Description()
                    });
                }
                sink.SetCompleted();
            }).AssertNoAwait();
            return sink.AsSource();
        }

        private void HandleError(Exception error)
        {
            if (error is ScanDriverUnknownException)
            {
                Log.ErrorException(error.Message, error.InnerException);
                errorOutput?.DisplayError(error.Message, error);
            }
            else
            {
                errorOutput?.DisplayError(error.Message);
            }
        }

        private void ShowOperation(ScanOperation op, ScanParams scanParams)
        {
            Task.Run(() =>
            {
                Invoker.Current.SafeInvoke(() =>
                {
                    if (scanParams.Modal)
                    {
                        operationProgress.ShowModalProgress(op);
                    }
                    else
                    {
                        operationProgress.ShowBackgroundProgress(op);
                    }
                });
            });
        }

        private void TranslateProgress(ScanController controller, ScanOperation op)
        {
            var smoothProgress = new SmoothProgress();
            controller.PageStart += (sender, args) => smoothProgress.Reset();
            controller.PageProgress += (sender, args) => smoothProgress.InputProgressChanged(args.Progress);
            controller.ScanEnd += (senders, args) => smoothProgress.Reset();
            smoothProgress.OutputProgressChanged += (sender, args) => op.Progress((int) Math.Round(args.Value * 1000), 1000);
        }

        private ScanOptions BuildOptions(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent)
        {
            var options = new ScanOptions
            {
                Driver = scanProfile.DriverName == DriverNames.WIA ? Driver.Wia
                    : scanProfile.DriverName == DriverNames.SANE ? Driver.Sane
                    : scanProfile.DriverName == DriverNames.TWAIN ? Driver.Twain
                    : Driver.Default,
                WiaOptions =
                {
                    WiaVersion = scanProfile.WiaVersion,
                    OffsetWidth = scanProfile.WiaOffsetWidth
                },
                TwainOptions =
                {
                    Adapter = scanProfile.TwainImpl == TwainImpl.Legacy ? TwainAdapter.Legacy : TwainAdapter.NTwain,
                    Dsm = scanProfile.TwainImpl == TwainImpl.X64 ? TwainDsm.NewX64
                        : scanProfile.TwainImpl == TwainImpl.OldDsm || scanProfile.TwainImpl == TwainImpl.Legacy ? TwainDsm.Old
                        : TwainDsm.New,
                    TransferMode = scanProfile.TwainImpl == TwainImpl.MemXfer ? TwainTransferMode.Memory : TwainTransferMode.Native,
                    IncludeWiaDevices = false
                },
                SaneOptions =
                {
                    KeyValueOptions = scanProfile.KeyValueOptions != null
                        ? new KeyValueScanOptions(scanProfile.KeyValueOptions)
                        : new KeyValueScanOptions()
                },
                NetworkOptions =
                {
                    Ip = scanProfile.ProxyConfig.Ip,
                    Port = scanProfile.ProxyConfig.Port
                },
                Brightness = scanProfile.Brightness,
                Contrast = scanProfile.Contrast,
                Dpi = scanProfile.Resolution.ToIntDpi(),
                Modal = scanParams.Modal,
                Quality = scanProfile.Quality,
                AutoDeskew = scanProfile.AutoDeskew,
                BitDepth = scanProfile.BitDepth.ToBitDepth(),
                DialogParent = dialogParent,
                DoOcr = scanParams.DoOcr,
                MaxQuality = scanProfile.MaxQuality,
                OcrParams = scanParams.OcrParams,
                PageAlign = scanProfile.PageAlign.ToHorizontalAlign(),
                PaperSource = scanProfile.PaperSource.ToPaperSource(),
                ScaleRatio = scanProfile.AfterScanScale.ToIntScaleFactor(),
                ThumbnailSize = scanParams.ThumbnailSize,
                DetectPatchCodes = scanParams.DetectPatchCodes,
                ExcludeBlankPages = scanProfile.ExcludeBlankPages,
                FlipDuplexedPages = scanProfile.FlipDuplexedPages,
                NoUI = scanParams.NoUI,
                OcrCancelToken = scanParams.OcrCancelToken,
                OcrInBackground = true, // TODO
                BlankPageCoverageThreshold = scanProfile.BlankPageCoverageThreshold,
                BlankPageWhiteThreshold = scanProfile.BlankPageWhiteThreshold,
                BrightnessContrastAfterScan = scanProfile.BrightnessContrastAfterScan,
                CropToPageSize = scanProfile.ForcePageSizeCrop,
                StretchToPageSize = scanProfile.ForcePageSize,
                UseNativeUI = scanProfile.UseNativeUI,
                Device = null, // Set after
                PageSize = null, // Set after
            };

            PageDimensions pageDimensions = scanProfile.PageSize.PageDimensions() ?? scanProfile.CustomPageSize;
            if (pageDimensions == null)
            {
                throw new ArgumentException("No page size specified");
            }

            options.PageSize = new PageSize(pageDimensions.Width, pageDimensions.Height, pageDimensions.Unit);

            return options;
        }

        private async Task<bool> PopulateDevice(ScanProfile scanProfile, ScanOptions options)
        {
            // If a device wasn't specified, prompt the user to pick one
            if (string.IsNullOrEmpty(scanProfile.Device?.ID))
            {
                options.Device = await PromptForDevice(options);
                if (options.Device == null)
                {
                    return false;
                }

                // Persist the device in the profile if configured to do so
                if (configProvider.Get(c => c.AlwaysRememberDevice))
                {
                    scanProfile.Device = options.Device;
                    profileManager.Save();
                }
            }
            else
            {
                options.Device = scanProfile.Device;
            }
            return true;
        }

        private async Task<ScanDevice> PromptForDevice(ScanOptions options)
        {
            if (options.Driver == Driver.Wia)
            {
                // WIA has a nice built-in device selection dialog, so use it
                using var deviceManager = new WiaDeviceManager(options.WiaOptions.WiaVersion);
                var wiaDevice = deviceManager.PromptForDevice(options.DialogParent);
                if (wiaDevice == null)
                {
                    return null;
                }

                return new ScanDevice(wiaDevice.Id(), wiaDevice.Name());
            }

            // Other drivers do not, so use a generic dialog
            var deviceForm = formFactory.Create<FSelectDevice>();
            deviceForm.DeviceList = await new ScanController().GetDeviceList(options);
            deviceForm.ShowDialog(new Win32Window(options.DialogParent));
            return deviceForm.SelectedDevice;
        }
    }
}
