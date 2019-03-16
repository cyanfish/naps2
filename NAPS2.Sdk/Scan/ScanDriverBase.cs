using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Config;
using NAPS2.Scan.Exceptions;
using NAPS2.Images;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Logging;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Scan
{
    /// <summary>
    /// A base class for IScanDriver implementing common error handling.
    /// </summary>
    public abstract class ScanDriverBase : IScanDriver
    {
        private readonly ErrorOutput errorOutput;
        private readonly AutoSaver autoSaver;

        protected ScanDriverBase(ErrorOutput errorOutput, AutoSaver autoSaver)
        {
            this.errorOutput = errorOutput;
            this.autoSaver = autoSaver;
        }

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
            Task.Run(async () =>
            {
                ScanDriverException error = null;
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
                    error = e;
                }
                // TODO
                //catch (FaultException<ScanDriverExceptionDetail> e)
                //{
                //    error = e.Detail.Exception;
                //}
                catch (Exception e)
                {
                    error = new ScanDriverUnknownException(e);
                }

                if (error != null)
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

                    if (scanParams.PropagateErrors)
                    {
                        sink.SetError(error);
                    }
                    else
                    {
                        sink.SetCompleted();
                    }
                }
            });
            return AutoSave(sink, scanParams, scanProfile);
        }

        private ScannedImageSource AutoSave(ScannedImageSink sink, ScanParams scanParams, ScanProfile scanProfile)
        {
            bool doAutoSave = !scanParams.NoAutoSave && !AppConfig.Current.DisableAutoSave && scanProfile.EnableAutoSave && scanProfile.AutoSaveSettings != null && autoSaver != null;
            if (!doAutoSave)
            {
                // No auto save, so just pipe images back as we get them
                return sink.AsSource();
            }
            return autoSaver.Save(scanProfile.AutoSaveSettings, sink.AsSource());
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
