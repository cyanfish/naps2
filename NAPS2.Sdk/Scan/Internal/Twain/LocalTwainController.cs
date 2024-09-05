#if !MAC
using System.Collections.Immutable;
using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Scan.Exceptions;
using NAPS2.Unmanaged;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Real implementation of ITwainController that interacts with a Twain session in the current process.
/// </summary>
internal class LocalTwainController : ITwainController
{
    public static readonly TWIdentity TwainAppId;

    static LocalTwainController()
    {
        PlatformInfo.Current.Log.IsDebugEnabled = true;
        TwainAppId = TWIdentity.Create(DataGroups.Image | DataGroups.Control, AssemblyHelper.Version,
            AssemblyHelper.Company, AssemblyHelper.Product, AssemblyHelper.Product, AssemblyHelper.Description);
    }

    private static readonly Once TwainDsmSetup = new(() =>
    {
        var twainDsmPath = NativeLibrary.FindLibraryPath("twaindsm.dll");
        PlatformCompat.System.LoadLibrary(twainDsmPath);
        PlatformInfo.Current.NewDsmPath = twainDsmPath;
    });

    private readonly ILogger _logger;

    public LocalTwainController(ScanningContext scanningContext)
    {
        _logger = scanningContext.Logger;
    }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        if (options.TwainOptions.Dsm != TwainDsm.Old)
        {
            TwainDsmSetup.Run();
        }
        return Task.Run(() =>
        {
            var deviceList = InternalGetDeviceList(options);
            if (options.TwainOptions.Dsm != TwainDsm.Old && deviceList.Count == 0)
            {
                // Fall back to OldDsm in case of no devices
                // This is primarily for Citrix support, which requires using twain_32.dll for TWAIN passthrough
                deviceList = InternalGetDeviceList(options);
            }

            return deviceList;
        });
    }

    private List<ScanDevice> InternalGetDeviceList(ScanOptions options)
    {
        PlatformInfo.Current.PreferNewDSM = options.TwainOptions.Dsm != TwainDsm.Old;
        var session = new TwainSession(TwainAppId);
        session.Open(TwainHandleManager.Factory().CreateMessageLoopHook());
        try
        {
            return session.GetSources().Select(ds => new ScanDevice(Driver.Twain, ds.Name, ds.Name)).ToList();
        }
        finally
        {
            try
            {
                session.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error closing TWAIN session");
            }
        }
    }

    public Task<ScanCaps> GetCaps(ScanOptions options)
    {
        if (options.TwainOptions.Dsm != TwainDsm.Old)
        {
            TwainDsmSetup.Run();
        }
        return Task.Run(() =>
        {
            var caps = InternalGetCaps(options);
            if (options.TwainOptions.Dsm != TwainDsm.Old && caps == null)
            {
                // Fall back to OldDsm in case of no devices
                // This is primarily for Citrix support, which requires using twain_32.dll for TWAIN passthrough
                caps = InternalGetCaps(options);
            }

            return caps;
        });
    }

    private ScanCaps InternalGetCaps(ScanOptions options)
    {
        PlatformInfo.Current.PreferNewDSM = options.TwainOptions.Dsm != TwainDsm.Old;
        var session = new TwainSession(TwainAppId);
        session.Open(TwainHandleManager.Factory().CreateMessageLoopHook());
        try
        {
            var ds = session.GetSources().FirstOrDefault(ds => ds.Name == options.Device!.ID);
            if (ds == null) throw new DeviceNotFoundException();
            try
            {
                var rc = ds.Open();
                if (rc != ReturnCode.Success)
                {
                    _logger.LogDebug("Couldn't open TWAIN data source for capabilities, return code {RC}", rc);
                    return new ScanCaps();
                }
                try
                {
                    var feederCap = ds.Capabilities.CapFeederEnabled;

                    feederCap.SetValue(BoolType.False);
                    bool supportsFlatbed = feederCap.GetCurrent() == BoolType.False;
                    var flatbedCaps = supportsFlatbed ? GetPerSourceCaps(ds) : null;

                    feederCap.SetValue(BoolType.True);
                    bool supportsFeeder = feederCap.GetCurrent() == BoolType.True;
                    var feederCaps = supportsFeeder ? GetPerSourceCaps(ds) : null;

                    bool supportsDuplex = supportsFeeder && ds.Capabilities.CapDuplex.GetCurrent() != Duplex.None;

                    return new ScanCaps
                    {
                        MetadataCaps = new MetadataCaps
                        {
                            Manufacturer = ds.Manufacturer,
                            Model = ds.Name,
                            SerialNumber = ds.Capabilities.CapSerialNumber.GetCurrent()
                        },
                        PaperSourceCaps = new PaperSourceCaps
                        {
                            SupportsFlatbed = supportsFlatbed,
                            SupportsFeeder = supportsFeeder,
                            SupportsDuplex = supportsDuplex,
                            CanCheckIfFeederHasPaper =
                                ds.Capabilities.CapAutomaticSenseMedium.IsSupported ||
                                ds.Capabilities.CapFeederLoaded.IsSupported
                        },
                        FlatbedCaps = flatbedCaps,
                        FeederCaps = feederCaps,
                        DuplexCaps = supportsDuplex ? feederCaps : null
                    };
                }
                finally
                {
                    ds.Close();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting TWAIN capabilities");
                if (e is DeviceException)
                {
                    throw;
                }
                throw new ScanDriverUnknownException(e);
            }
        }
        finally
        {
            try
            {
                session.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error closing TWAIN session for capabilities");
            }
        }
    }

    private PerSourceCaps GetPerSourceCaps(DataSource ds)
    {
        var xRes = ds.Capabilities.ICapXResolution.GetValues().Select(x => (int) x.Whole);
        var yRes = ds.Capabilities.ICapYResolution.GetValues().Select(x => (int) x.Whole);
        var dpiCaps = new DpiCaps { Values = xRes.Intersect(yRes).ToImmutableList() };
        var pixelTypes = ds.Capabilities.ICapPixelType.GetValues().ToList();
        var bitDepthCaps = new BitDepthCaps
        {
            SupportsColor = pixelTypes.Contains(PixelType.RGB),
            SupportsGrayscale = pixelTypes.Contains(PixelType.Gray),
            SupportsBlackAndWhite = pixelTypes.Contains(PixelType.BlackWhite)
        };
        var w = ds.Capabilities.ICapPhysicalWidth.GetCurrent();
        var h = ds.Capabilities.ICapPhysicalHeight.GetCurrent();
        var scanArea = new PageSize(
            decimal.Round(w.Whole + w.Fraction / 65536m, 4),
            decimal.Round(h.Whole + h.Fraction / 65536m, 4),
            PageSizeUnit.Inch);
        var pageSizeCaps = new PageSizeCaps { ScanArea = scanArea };
        return new PerSourceCaps
        {
            DpiCaps = dpiCaps,
            BitDepthCaps = bitDepthCaps,
            PageSizeCaps = pageSizeCaps
        };
    }

    public async Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        if (options.TwainOptions.Dsm != TwainDsm.Old)
        {
            TwainDsmSetup.Run();
        }
        try
        {
            await InternalScan(options.TwainOptions.Dsm, options, cancelToken, twainEvents);
        }
        catch (DeviceNotFoundException)
        {
            if (options.TwainOptions.Dsm != TwainDsm.Old)
            {
                // Fall back to OldDsm in case of no devices
                // This is primarily for Citrix support, which requires using twain_32.dll for TWAIN passthrough
                await InternalScan(TwainDsm.Old, options, cancelToken, twainEvents);
            }
            else
            {
                throw;
            }
        }
    }

    private async Task InternalScan(TwainDsm dsm, ScanOptions options, CancellationToken cancelToken,
        ITwainEvents twainEvents)
    {
        var runner = new TwainScanRunner(_logger, TwainAppId, dsm, options, cancelToken, twainEvents);
        await runner.Run();
    }
}
#endif