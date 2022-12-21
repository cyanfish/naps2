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

    private readonly EsclServerConfig _serverConfig;
    private readonly EsclServerState _serverState;

    internal EsclApiController(EsclServerConfig serverConfig, EsclServerState serverState)
    {
        _serverConfig = serverConfig;
        _serverState = serverState;
    }

    [Route(HttpVerbs.Get, "/ScannerCapabilities")]
    public async Task GetScannerCapabilities()
    {
        var caps = _serverConfig.Capabilities;
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScannerCapabilities",
                    new XElement(PwgNs + "Version", caps.Version), // TODO: Probably hard code version or something
                    new XElement(PwgNs + "MakeAndModel", caps.MakeAndModel),
                    new XElement(PwgNs + "SerialNumber", caps.SerialNumber),
                    new XElement(ScanNs + "UUID", "0e468f6d-e5dc-4abe-8e9f-ad08d8546b0c"),
                    new XElement(ScanNs + "AdminURI", ""),
                    new XElement(ScanNs + "IconURI", ""),
                    new XElement(ScanNs + "SettingProfiles",
                        new XElement(ScanNs + "SettingProfile",
                            new XAttribute("name", "p1"),
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
                                    new XElement(ScanNs + "DiscreteResolution",
                                        new XElement(ScanNs + "XResolution", "100"),
                                        new XElement(ScanNs + "YResolution", "100")))))),
                    new XElement(ScanNs + "Platen",
                        new XElement(ScanNs + "PlatenInputCaps",
                            new XElement(ScanNs + "MinWidth", "1"),
                            new XElement(ScanNs + "MaxWidth", "3000"),
                            new XElement(ScanNs + "MinHeight", "1"),
                            new XElement(ScanNs + "MaxHeight", "3600"),
                            new XElement(ScanNs + "MaxScanRegions", "1"),
                            new XElement(ScanNs + "SettingProfiles",
                                new XElement(ScanNs + "SettingProfile",
                                    new XAttribute("ref", "p1")))))));
        Response.ContentType = "text/xml";
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc);
    }

    [Route(HttpVerbs.Get, "/ScannerStatus")]
    public async Task GetScannerStatus()
    {
        var jobsElement = new XElement(ScanNs + "Jobs");
        foreach (var jobState in _serverState.Jobs.Values)
        {
            jobsElement.Add(new XElement(ScanNs + "JobInfo",
                new XElement(PwgNs + "JobUri", $"/escl/ScanJobs/{jobState.Id}"),
                new XElement(PwgNs + "JobUuid", jobState.Id),
                new XElement(ScanNs + "Age", Math.Ceiling(jobState.LastUpdated.Elapsed.TotalSeconds)),
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
                    new XElement(PwgNs + "State",
                        _serverState.Jobs.Any(x => x.Value.Status is JobStatus.Pending or JobStatus.Processing)
                            ? "Processing"
                            : "Idle"),
                    jobsElement
                ));
        Response.ContentType = "text/xml";
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc);
    }

    [Route(HttpVerbs.Post, "/ScanJobs")]
    public void CreateScanJob()
    {
        var jobState = JobState.CreateNewJob();
        _serverState.Jobs[jobState.Id] = jobState;
        Response.Headers.Add("Location", $"{Request.Url}/{jobState.Id}");
        Response.StatusCode = 201;
    }

    [Route(HttpVerbs.Delete, "/ScanJobs/{jobId}")]
    public void CancelScanJob(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.Status is JobStatus.Pending or JobStatus.Processing)
        {
            jobState.Status = JobStatus.Canceled;
            jobState.LastUpdated = Stopwatch.StartNew();
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

    [Route(HttpVerbs.Get, "/ScanJobs/{jobId}/NextDocument")]
    public void NextDocument(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState) &&
            jobState.Status is JobStatus.Pending or JobStatus.Processing)
        {
            Response.Headers.Add("Content-Location", $"/escl/ScanJobs/{jobState.Id}/1");
            // Bypass https://github.com/unosquare/embedio/issues/510
            var field = Response.GetType().GetField("<ProtocolVersion>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field!.SetValue(Response, new Version(1, 1));
            Response.SendChunked = true;
            Response.ContentType = "image/jpeg";
            Response.ContentEncoding = null;
            using var stream = Response.OutputStream;
            var bytes = File.ReadAllBytes(@"C:\Devel\VS\NAPS2.Future\NAPS2.Sdk.Tests\Resources\dog.jpg");
            stream.Write(bytes, 0, bytes.Length);
            jobState.Status = JobStatus.Completed;
            jobState.LastUpdated = Stopwatch.StartNew();
        }
        else
        {
            Response.StatusCode = 404;
        }
    }
}