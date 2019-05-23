using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.Scan
{
    /// <summary>
    /// A high-level interface used for scanning.
    /// This abstracts away the logic of obtaining and using an instance of IScanDriver.
    /// </summary>
    public class ScanPerformer : IScanPerformer
    {
        private readonly IScanDriverFactory driverFactory;
        private readonly IErrorOutput errorOutput;
        private readonly IAutoSave autoSave;
        private readonly AppConfigManager appConfigManager;
        private readonly IProfileManager profileManager;
        private readonly ScannedImageHelper scannedImageHelper;

        public ScanPerformer(IScanDriverFactory driverFactory, IErrorOutput errorOutput, IAutoSave autoSave, AppConfigManager appConfigManager, IProfileManager profileManager, ScannedImageHelper scannedImageHelper)
        {
            this.driverFactory = driverFactory;
            this.errorOutput = errorOutput;
            this.autoSave = autoSave;
            this.appConfigManager = appConfigManager;
            this.profileManager = profileManager;
            this.scannedImageHelper = scannedImageHelper;
        }

        public async Task PerformScan(ScanProfile scanProfile, ScanParams scanParams, IWin32Window dialogParent, ISaveNotify notify,
            Action<ScannedImage> imageCallback, CancellationToken cancelToken = default)
        {
            var driver = driverFactory.Create(scanProfile.DriverName);
            driver.DialogParent = dialogParent;
            driver.ScanProfile = scanProfile;
            driver.ScanParams = scanParams;
            driver.CancelToken = cancelToken;
            try
            {
                if (scanProfile.Device == null)
                {
                    // The profile has no device specified, so prompt the user to choose one
                    var device = driver.PromptForDevice();
                    if (device == null)
                    {
                        // User cancelled
                        return;
                    }
                    if (appConfigManager.Config.AlwaysRememberDevice)
                    {
                        scanProfile.Device = device;
                        profileManager.Save();
                    }
                    driver.ScanDevice = device;
                }
                else
                {
                    // The profile has a device specified, so use it
                    driver.ScanDevice = scanProfile.Device;
                }

                // Start the scan
                int imageCount = 0;
                var source = driver.Scan().Then(img => imageCount++);

                bool doAutoSave = !scanParams.NoAutoSave && !appConfigManager.Config.DisableAutoSave && scanProfile.EnableAutoSave && scanProfile.AutoSaveSettings != null;
                if (doAutoSave)
                {
                    if (scanProfile.AutoSaveSettings.ClearImagesAfterSaving)
                    {
                        // Auto save without piping images
                        var images = await source.ToList();
                        if (await autoSave.Save(scanProfile.AutoSaveSettings, images, notify))
                        {
                            foreach (ScannedImage img in images)
                            {
                                img.Dispose();
                            }
                        }
                        else
                        {
                            // Fallback in case auto save failed; pipe all the images back at once
                            foreach (ScannedImage img in images)
                            {
                                imageCallback(img);
                            }
                        }
                    }
                    else
                    {
                        // Basic auto save, so keep track of images as we pipe them and try to auto save afterwards
                        var images = new List<ScannedImage>();
                        await source.ForEach(scannedImage =>
                        {
                            imageCallback(scannedImage);
                            images.Add(scannedImage);
                        });
                        await autoSave.Save(scanProfile.AutoSaveSettings, images, notify);
                    }
                }
                else
                {
                    // No auto save, so just pipe images back as we get them
                    await source.ForEach(imageCallback);
                }

                if (imageCount > 0)
                {
                    Log.Event(EventType.Scan, new Event
                    {
                        Name = MiscResources.Scan,
                        Pages = imageCount,
                        DeviceName = scanProfile.Device?.Name,
                        ProfileName = scanProfile.DisplayName,
                        BitDepth = scanProfile.BitDepth.Description()
                    });
                }
            }
            catch (ScanDriverException e)
            {
                if (e is ScanDriverUnknownException)
                {
                    Log.ErrorException(e.Message, e.InnerException);
                    errorOutput.DisplayError(e.Message, e);
                }
                else
                {
                    errorOutput.DisplayError(e.Message);
                }
            }
        }
    }
}
