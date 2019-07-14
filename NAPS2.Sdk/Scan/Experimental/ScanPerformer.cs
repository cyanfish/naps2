using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Operation;
using NAPS2.Scan.Wia.Native;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan.Experimental
{
    public class ScanPerformer : IScanPerformer
    {
        private readonly IFormFactory formFactory;
        private readonly ConfigProvider<CommonConfig> configProvider;
        private readonly OperationProgress operationProgress;
        private readonly AutoSaver autoSaver;
        private readonly IProfileManager profileManager;

        public ScanPerformer(IFormFactory formFactory, ConfigProvider<CommonConfig> configProvider, OperationProgress operationProgress, AutoSaver autoSaver, IProfileManager profileManager)
        {
            this.formFactory = formFactory;
            this.configProvider = configProvider;
            this.operationProgress = operationProgress;
            this.autoSaver = autoSaver;
            this.profileManager = profileManager;
        }

        public ScannedImageSource PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default,
            CancellationToken cancelToken = default)
        {
            var options = BuildOptions(scanProfile, scanParams, dialogParent);
            if (options == null)
            {
                // User cancelled out of a dialog
                return ScannedImageSource.Empty;
            }
            var controller = new ScanController();
            var op = new ScanOperation(options.Device, options.PaperSource);

            controller.PageStart += (sender, args) => op.NextPage(args.PageNumber);
            controller.ScanEnd += (sender, args) => op.Completed();
            TranslateProgress(controller, op);

            ShowOperation(op, scanParams);
            cancelToken.Register(op.Cancel);

            var source = controller.Scan(options, op.CancelToken);

            if (scanProfile.AutoSaveSettings != null)
            {
                source = autoSaver.Save(scanProfile.AutoSaveSettings, source);
            }
            return source;
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
                Driver = scanProfile.DriverName == "wia" ? Driver.Wia
                       : scanProfile.DriverName == "sane" ? Driver.Sane
                       : scanProfile.DriverName == "twain" ? Driver.Twain
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
                    KeyValueOptions = new KeyValueScanOptions(scanProfile.KeyValueOptions)
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

            // If a device wasn't specified, prompt the user to pick one
            if (string.IsNullOrEmpty(scanProfile.Device?.ID))
            {
                if (options.Driver == Driver.Wia)
                {
                    // WIA has a nice built-in device selection dialog, so use it
                    using (var deviceManager = new WiaDeviceManager(options.WiaOptions.WiaVersion))
                    {
                        var wiaDevice = deviceManager.PromptForDevice(dialogParent);
                        if (wiaDevice == null)
                        {
                            return null;
                        }
                        options.Device = new ScanDevice(wiaDevice.Id(), wiaDevice.Name());
                    }
                }
                else
                {
                    // Other drivers do not, so use a generic dialog
                    var deviceForm = formFactory.Create<FSelectDevice>();
                    deviceForm.DeviceList = new ScanController().GetDeviceList(options);
                    deviceForm.ShowDialog(new Win32Window(dialogParent));
                    if (deviceForm.SelectedDevice == null)
                    {
                        return null;
                    }
                    options.Device = deviceForm.SelectedDevice;
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

            return options;
        }
    }
}
