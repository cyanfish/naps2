using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.WinForms;
using NAPS2.Worker;

namespace NAPS2.Scan
{
    /// <summary>
    /// A base class for IScanDriver implementing common error handling.
    /// </summary>
    public abstract class ScanDriverBase : IScanDriver
    {
        public abstract string DriverName { get; }

        public abstract bool IsSupported { get; }
        
        public ScanDevice PromptForDevice(ScanProfile scanProfile, IntPtr dialogParent = default)
        {
            if (!IsSupported)
            {
                throw new DriverNotSupportedException();
            }
            try
            {
                return PromptForDeviceInternal(scanProfile, dialogParent);
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        protected virtual ScanDevice PromptForDeviceInternal(ScanProfile scanProfile, IntPtr dialogParent)
        {
            var deviceList = GetDeviceList(scanProfile);

            if (!deviceList.Any())
            {
                throw new NoDevicesFoundException();
            }

            var form = new FSelectDevice
            {
                DeviceList = deviceList
            };
            form.ShowDialog();
            return form.SelectedDevice;
        }

        public List<ScanDevice> GetDeviceList(ScanProfile scanProfile)
        {
            if (!IsSupported)
            {
                throw new DriverNotSupportedException();
            }
            try
            {
                return GetDeviceListInternal(scanProfile);
            }
            catch (ScanDriverException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScanDriverUnknownException(e);
            }
        }

        protected abstract List<ScanDevice> GetDeviceListInternal(ScanProfile scanProfile);

        public ScannedImageSource Scan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default)
        {
            if (!IsSupported)
            {
                throw new DriverNotSupportedException();
            }
            if (scanProfile == null)
            {
                throw new ArgumentNullException(nameof(scanProfile));
            }
            if (scanParams == null)
            {
                throw new ArgumentNullException(nameof(scanParams));
            }

            var sink = new ScannedImageSink();
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    var device = GetScanDevice(scanProfile);
                    if (device != null)
                    {
                        await ScanInternal(sink, device, scanProfile, scanParams, dialogParent, cancelToken);
                    }

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
                }
                catch (ScanDriverException e)
                {
                    sink.SetError(e);
                }
                catch (FaultException<ScanDriverExceptionDetail> e)
                {
                    sink.SetError(e.Detail.Exception);
                }
                catch (Exception e)
                {
                    sink.SetError(new ScanDriverUnknownException(e));
                }
            }, TaskCreationOptions.LongRunning);
            return sink.AsSource();
        }

        private void AutoSaveStuff()
        {
            // TODO
            //bool doAutoSave = !scanParams.NoAutoSave && !AppConfig.Current.DisableAutoSave && scanProfile.EnableAutoSave && scanProfile.AutoSaveSettings != null;
            //if (doAutoSave)
            //{
            //    if (scanProfile.AutoSaveSettings.ClearImagesAfterSaving)
            //    {
            //        // Auto save without piping images
            //        var images = await source.ToList();
            //        if (await autoSave.Save(scanProfile.AutoSaveSettings, images, notify))
            //        {
            //            foreach (ScannedImage img in images)
            //            {
            //                img.Dispose();
            //            }
            //        }
            //        else
            //        {
            //            // Fallback in case auto save failed; pipe all the images back at once
            //            foreach (ScannedImage img in images)
            //            {
            //                imageCallback(img);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        // Basic auto save, so keep track of images as we pipe them and try to auto save afterwards
            //        var images = new List<ScannedImage>();
            //        await source.ForEach(scannedImage =>
            //        {
            //            imageCallback(scannedImage);
            //            images.Add(scannedImage);
            //        });
            //        await autoSave.Save(scanProfile.AutoSaveSettings, images, notify);
            //    }
            //}
            //else
            //{
            //    // No auto save, so just pipe images back as we get them
            //    await source.ForEach(imageCallback);
            //}
        }

        private ScanDevice GetScanDevice(ScanProfile scanProfile)
        {
            if (scanProfile.Device != null)
            {
                // The profile has a device specified, so use it
                return scanProfile.Device;
            }

            // The profile has no device specified, so prompt the user to choose one
            var device = PromptForDevice(scanProfile);
            if (device == null)
            {
                // User cancelled
                return null;
            }
            if (AppConfig.Current.AlwaysRememberDevice)
            {
                scanProfile.Device = device;
                ProfileManager.Current.Save();
            }
            return device;
        }

        protected abstract Task ScanInternal(ScannedImageSink sink, ScanDevice scanDevice, ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent, CancellationToken cancelToken);
    }
}
