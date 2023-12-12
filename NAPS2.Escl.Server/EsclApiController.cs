using System.Reflection;
using System.Xml.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Microsoft.Extensions.Logging;

namespace NAPS2.Escl.Server;

internal class EsclApiController : WebApiController
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    private readonly EsclDeviceConfig _deviceConfig;
    private readonly EsclServerState _serverState;
    private readonly ILogger _logger;

    internal EsclApiController(EsclDeviceConfig deviceConfig, EsclServerState serverState, ILogger logger)
    {
        _deviceConfig = deviceConfig;
        _serverState = serverState;
        _logger = logger;
    }

    [Route(HttpVerbs.Get, "/ScannerCapabilities")]
    public async Task GetScannerCapabilities()
    {
        var caps = _deviceConfig.Capabilities;
        var iconUri = caps.IconPng != null ? $"http://naps2-{caps.Uuid}.local.:{_deviceConfig.Port}/eSCL/icon.png" : "";
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScannerCapabilities",
                    new XElement(PwgNs + "Version", caps.Version),
                    new XElement(PwgNs + "MakeAndModel", caps.MakeAndModel),
                    new XElement(PwgNs + "SerialNumber", caps.SerialNumber),
                    new XElement(ScanNs + "UUID", caps.Uuid),
                    new XElement(ScanNs + "AdminURI", ""),
                    new XElement(ScanNs + "IconURI", iconUri),
                    new XElement(ScanNs + "Naps2Extensions", "Progress;ErrorDetails"),
                    new XElement(ScanNs + "Platen",
                        new XElement(ScanNs + "PlatenInputCaps", GetCommonInputCaps())),
                    new XElement(ScanNs + "Adf",
                        new XElement(ScanNs + "AdfSimplexInputCaps", GetCommonInputCaps()),
                        new XElement(ScanNs + "AdfDuplexInputCaps", GetCommonInputCaps()))));
        Response.ContentType = "text/xml";
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc);
    }

    private object[] GetCommonInputCaps()
    {
        // TODO: After implementing scanner capabilities this should be scanner-specific
        return
        [
            new XElement(ScanNs + "MinWidth", "1"),
            new XElement(ScanNs + "MaxWidth", EsclInputCaps.DEFAULT_MAX_WIDTH),
            new XElement(ScanNs + "MinHeight", "1"),
            new XElement(ScanNs + "MaxHeight", EsclInputCaps.DEFAULT_MAX_HEIGHT),
            new XElement(ScanNs + "MaxScanRegions", "1"),
            new XElement(ScanNs + "SettingProfiles",
                new XElement(ScanNs + "SettingProfile",
                    new XElement(ScanNs + "ColorModes",
                        new XElement(ScanNs + "ColorMode", "BlackAndWhite1"),
                        new XElement(ScanNs + "ColorMode", "Grayscale8"),
                        new XElement(ScanNs + "ColorMode", "RGB24")),
                    new XElement(ScanNs + "DocumentFormats",
                        new XElement(PwgNs + "DocumentFormat", "application/pdf"),
                        new XElement(PwgNs + "DocumentFormat", "image/jpeg"),
                        new XElement(PwgNs + "DocumentFormat", "image/png"),
                        new XElement(ScanNs + "DocumentFormatExt", "application/pdf"),
                        new XElement(ScanNs + "DocumentFormatExt", "image/jpeg"),
                        new XElement(ScanNs + "DocumentFormatExt", "image/png")
                    ),
                    new XElement(ScanNs + "SupportedResolutions",
                        new XElement(ScanNs + "DiscreteResolutions",
                            CreateResolution(100),
                            CreateResolution(150),
                            CreateResolution(200),
                            CreateResolution(300),
                            CreateResolution(400),
                            CreateResolution(600),
                            CreateResolution(800),
                            CreateResolution(1200),
                            CreateResolution(2400),
                            CreateResolution(4800)
                        ))))
        ];
    }

    private XElement CreateResolution(int res) =>
        new(ScanNs + "DiscreteResolution",
            new XElement(ScanNs + "XResolution", res.ToString()),
            new XElement(ScanNs + "YResolution", res.ToString()));

    [Route(HttpVerbs.Get, "/icon.png")]
    public async Task GetIcon()
    {
        if (_deviceConfig.Capabilities.IconPng != null)
        {
            Response.ContentType = "image/png";
            using var stream = Response.OutputStream;
            var buffer = _deviceConfig.Capabilities.IconPng;
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
        else
        {
            Response.StatusCode = 404;
        }
    }

    [Route(HttpVerbs.Get, "/ScannerStatus")]
    public async Task GetScannerStatus()
    {
        var jobsElement = new XElement(ScanNs + "Jobs");
        foreach (var jobInfo in _serverState.Jobs.Values)
        {
            jobsElement.Add(new XElement(ScanNs + "JobInfo",
                new XElement(PwgNs + "JobUri", $"/eSCL/ScanJobs/{jobInfo.Id}"),
                new XElement(PwgNs + "JobUuid", jobInfo.Id),
                new XElement(ScanNs + "Age", Math.Ceiling(jobInfo.LastUpdated.Elapsed.TotalSeconds)),
                // TODO: real data
                new XElement(PwgNs + "ImagesCompleted",
                    jobInfo.State is EsclJobState.Pending or EsclJobState.Processing ? "0" : "1"),
                new XElement(PwgNs + "ImagesToTransfer", "1"),
                new XElement(PwgNs + "JobState", jobInfo.State.ToString()),
                new XElement(PwgNs + "JobStateReasons",
                    new XElement(PwgNs + "JobStateReason",
                        jobInfo.State == EsclJobState.Processing ? "JobScanning" : "JobCompletedSuccessfully"))));
        }
        var scannerState = _serverState.IsProcessing ? EsclScannerState.Processing : EsclScannerState.Idle;
        var adfState = _serverState.IsProcessing ? EsclAdfState.ScannerAdfProcessing : EsclAdfState.ScannedAdfLoaded;
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScannerStatus",
                    new XElement(PwgNs + "Version", "2.6"),
                    new XElement(PwgNs + "State", scannerState),
                    new XElement(ScanNs + "AdfState", adfState),
                    jobsElement
                ));
        Response.ContentType = "text/xml";
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc);
    }

    [Route(HttpVerbs.Post, "/ScanJobs")]
    public void CreateScanJob()
    {
        // TODO: Actually use job input for scan options
        EsclScanSettings settings;
        try
        {
            var doc = XDocument.Load(Request.InputStream);
            settings = SettingsParser.Parse(doc);
        }
        catch (Exception)
        {
            Response.StatusCode = 400; // Bad request
            return;
        }
        if (_serverState.IsProcessing)
        {
            Response.StatusCode = 503; // Service unavailable
            return;
        }
        _serverState.IsProcessing = true;
        var jobState = JobInfo.CreateNewJob(_serverState, _deviceConfig.CreateJob(settings));
        _serverState.Jobs[jobState.Id] = jobState;
        Response.Headers.Add("Location", $"{Request.Url}/{jobState.Id}");
        Response.StatusCode = 201; // Created
    }

    [Route(HttpVerbs.Delete, "/ScanJobs/{jobId}")]
    public void CancelScanJob(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.State is EsclJobState.Pending or EsclJobState.Processing)
        {
            jobState.Job.Cancel();
        }
        else
        {
            Response.StatusCode = 404;
        }
    }

    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/ScanImageInfo")]
    public void GetImageinfo(string jobId)
    {
    }

    // This endpoint is a NAPS2-specific extension to the ESCL API.
    // It gives a chunked response where each line is a double between 0 and 1 representing the current page progress.
    // This endpoint should be called once before each call to NextDocument.
    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/Progress")]
    public async Task Progress(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.State is EsclJobState.Pending or EsclJobState.Processing)
        {
            SetChunkedResponse();
            using var stream = Response.OutputStream;
            await jobState.Job.WriteProgressTo(stream);
        }
        else
        {
            Response.StatusCode = 404;
        }
    }

    // This endpoint is a NAPS2-specific extension to the ESCL API.
    // The ESCL status-based model (where errors like "no paper in feeder" are encompassed by ScannerStatus that can be
    // polled every few seconds) is a good match to a physical scanner but a poor match to NAPS2's model.
    // Instead, we have a this ErrorDetails endpoint that we call when NextDocument returns a 500 error which gives us
    // XML-based details for the exception that occurred.
    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/ErrorDetails")]
    public async Task ErrorDetails(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState))
        {
            Response.ContentType = "text/xml";
            using var stream = Response.OutputStream;
            await jobState.Job.WriteErrorDetailsTo(stream);
        }
        else
        {
            Response.StatusCode = 404;
        }
    }

    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/NextDocument")]
    public async Task NextDocument(string jobId)
    {
        if (!_serverState.Jobs.TryGetValue(jobId, out var jobInfo))
        {
            Response.StatusCode = 404;
            return;
        }
        if (jobInfo.State == EsclJobState.Aborted)
        {
            Response.StatusCode = 500;
            return;
        }
        if (jobInfo.State is not (EsclJobState.Pending or EsclJobState.Processing))
        {
            Response.StatusCode = 404;
            return;
        }

        bool result;
        try
        {
            result = await jobInfo.Job.WaitForNextDocument();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "ESCL server error waiting for document");
            // The ScanJob should transition the job to aborted on error, but there's a race condition where this could
            // happen first and another NextDocument call will think the job is still processing unless we ensure it's
            // in the correct state here.
            jobInfo.TransitionState(EsclJobState.Processing, EsclJobState.Aborted);
            Response.StatusCode = 500;
            return;
        }
        if (result)
        {
            Response.Headers.Add("Content-Location", $"/eSCL/ScanJobs/{jobInfo.Id}/1");
            SetChunkedResponse();
            Response.ContentType = jobInfo.Job.ContentType;
            Response.ContentEncoding = null;
            using var stream = Response.OutputStream;
            await jobInfo.Job.WriteDocumentTo(stream);
        }
        else
        {
            jobInfo.TransitionState(EsclJobState.Processing, EsclJobState.Completed);
            Response.StatusCode = 404;
        }
    }

    private void SetChunkedResponse()
    {
        // Bypass https://github.com/unosquare/embedio/issues/510
        var field = Response.GetType().GetField("<ProtocolVersion>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(Response, new Version(1, 1));
        }
        Response.SendChunked = true;
    }
}