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
                    new XElement(ScanNs + "UUID", Guid.NewGuid().ToString("D")),
                    new XElement(ScanNs + "AdminURI", ""),
                    new XElement(ScanNs + "IconURI", ""),
                    new XElement(ScanNs + "SettingProfiles",
                        new XElement(ScanNs + "SettingProfile",
                            new XAttribute("name", "p1"),
                            new XElement(ScanNs + "ColorModes",
                                new XElement(ScanNs + "ColorMode", "BlackAndWhite1"),
                                new XElement(ScanNs + "ColorMode", "Grayscale8")),
                            new XElement(ScanNs + "DocumentFormats",
                                new XElement(PwgNs + "DocumentFormat", "application/pdf"),
                                new XElement(PwgNs + "DocumentFormat", "image/jpeg"),
                                new XElement(PwgNs + "DocumentFormat", "image/png"),
                                new XElement(ScanNs + "DocumentFormatExt", "application/pdf"),
                                new XElement(ScanNs + "DocumentFormatExt", "image/jpeg"),
                                new XElement(ScanNs + "DocumentFormatExt", "image/png")),
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
                new XElement(PwgNs + "JobUri", $"/ScanJobs/{jobState.Id}"),
                new XElement(PwgNs + "JobUuid", jobState.Id),
                new XElement(ScanNs + "Age", "10"), // TODO: ?
                new XElement(PwgNs + "ImagesCompleted", jobState.Status == JobStatus.Completed ? 1 : 0),
                new XElement(PwgNs + "ImagesToTransfer", "1"),
                new XElement(PwgNs + "JobState", jobState.Status.ToString())));
        }
        var doc =
            EsclXmlHelper.CreateDocAsString(
                new XElement(ScanNs + "ScannerStatus",
                    new XElement(PwgNs + "Version", "2.6"),
                    new XElement(PwgNs + "State", _serverState.Jobs.Any(x => x.Value.Status is JobStatus.Pending or JobStatus.Processing) ? "Processing" : "Idle"),
                    jobsElement
                ));
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc);
    }

    [Route(HttpVerbs.Post, "/ScanJobs")]
    public async Task CreateScanJob()
    {
        var jobState = JobState.CreateNewJob();
        _serverState.Jobs[jobState.Id] = jobState;
        Response.Headers.Add("Location", $"{Request.RawUrl}/{jobState.Id}");
        Response.StatusCode = 201;
    }

    [Route(HttpVerbs.Delete, "/ScanJobs/{jobId}")]
    public async Task CancelScanJob(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState))
        {
            jobState.Status = JobStatus.Canceled;
        }
        else
        {
            Response.StatusCode = 404;
        }
    }

    [Route(HttpVerbs.Get, "/{jobId}/NextDocument")]
    public async Task NextDocument(string jobId)
    {
        if (_serverState.Jobs.TryGetValue(jobId, out var jobState))
        {
            Response.Headers.Add("Content-Location", $"/escl/ScanJobs/{jobState.Id}/1");
            Response.SendChunked = true;
            using var stream = HttpContext.OpenResponseStream();
            var bytes = File.ReadAllBytes(@"C:\Devel\VS\NAPS2.Future\NAPS2.Sdk.Tests\Resources\color_image.jpg");
            await stream.WriteAsync(bytes, 0, bytes.Length);
            jobState.Status = JobStatus.Completed;
        }
        else
        {
            Response.StatusCode = 404;
        }
    }
}