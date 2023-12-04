using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

internal class EsclApiController : WebApiController
{
    private static readonly XNamespace ScanNs = EsclXmlHelper.ScanNs;
    private static readonly XNamespace PwgNs = EsclXmlHelper.PwgNs;

    private readonly EsclDeviceConfig _deviceConfig;
    private readonly EsclServerState _serverState;

    internal EsclApiController(EsclDeviceConfig deviceConfig, EsclServerState serverState)
    {
        _deviceConfig = deviceConfig;
        _serverState = serverState;
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
                    new XElement(ScanNs + "Naps2Extensions", "Progress"),
                    new XElement(ScanNs + "Platen",
                        new XElement(ScanNs + "PlatenInputCaps",
                            new XElement(ScanNs + "MinWidth", "1"),
                            new XElement(ScanNs + "MaxWidth", "3000"),
                            new XElement(ScanNs + "MinHeight", "1"),
                            new XElement(ScanNs + "MaxHeight", "3600"),
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
                                        ))))))));
        Response.ContentType = "text/xml";
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc);
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
        foreach (var jobState in _serverState.Jobs.Values)
        {
            jobsElement.Add(new XElement(ScanNs + "JobInfo",
                new XElement(PwgNs + "JobUri", $"/eSCL/ScanJobs/{jobState.Id}"),
                new XElement(PwgNs + "JobUuid", jobState.Id),
                new XElement(ScanNs + "Age", Math.Ceiling(jobState.LastUpdated.Elapsed.TotalSeconds)),
                // TODO: real data
                new XElement(PwgNs + "ImagesCompleted",
                    jobState.Status is JobStatus.Pending or JobStatus.Processing ? "0" : "1"),
                new XElement(PwgNs + "ImagesToTransfer", "1"),
                new XElement(PwgNs + "JobState", jobState.Status.ToString()),
                new XElement(PwgNs + "JobStateReasons",
                    new XElement(PwgNs + "JobStateReason",
                        jobState.Status == JobStatus.Processing ? "JobScanning" : "JobCompletedSuccessfully"))));
        }
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScannerStatus",
                    new XElement(PwgNs + "Version", "2.6"),
                    new XElement(PwgNs + "State", _serverState.IsProcessing ? "Processing" : "Idle"),
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
        var jobState = JobState.CreateNewJob(_serverState, _deviceConfig.CreateJob(settings));
        _serverState.Jobs[jobState.Id] = jobState;
        Response.Headers.Add("Location", $"{Request.Url}/{jobState.Id}");
        Response.StatusCode = 201; // Created
    }

    [Route(HttpVerbs.Delete, "/ScanJobs/{jobId}")]
    public void CancelScanJob(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.Status is JobStatus.Pending or JobStatus.Processing)
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

    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/Progress")]
    public async Task Progress(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.Status is JobStatus.Pending or JobStatus.Processing)
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

    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/NextDocument")]
    public async Task NextDocument(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.Status is JobStatus.Pending or JobStatus.Processing)
        {
            if (await jobState.Job.WaitForNextDocument())
            {
                Response.Headers.Add("Content-Location", $"/eSCL/ScanJobs/{jobState.Id}/1");
                SetChunkedResponse();
                Response.ContentType = "image/jpeg";
                Response.ContentEncoding = null;
                using var stream = Response.OutputStream;
                jobState.Job.WriteDocumentTo(stream);
            }
            else
            {
                jobState.Status = JobStatus.Completed;
                Response.StatusCode = 404;
            }
        }
        else
        {
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