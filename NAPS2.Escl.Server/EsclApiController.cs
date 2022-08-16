using System.Xml.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace NAPS2.Escl.Server;

public class EsclApiController : WebApiController
{
    private static readonly XNamespace ScanNs = XNamespace.Get("http://schemas.hp.com/imaging/escl/2011/05/03");
    private static readonly XNamespace PwgNs = XNamespace.Get("http://www.pwg.org/schemas/2010/12/sm");

    private readonly EsclServerConfig _serverConfig;

    public EsclApiController(EsclServerConfig serverConfig)
    {
        _serverConfig = serverConfig;
    }

    [Route(HttpVerbs.Get, "/ScannerCapabilities")]
    public async Task GetScannerCapabilities()
    {
        var caps = _serverConfig.Capabilities;
        var doc =
            new XDocument(
                new XElement(ScanNs + "ScannerCapabilities",
                    new XElement(PwgNs + "Version", caps.Version), // TODO: Probably hard code version or something
                    new XElement(PwgNs + "MakeAndModel", caps.MakeAndModel),
                    new XElement(PwgNs + "SerialNumber", caps.SerialNumber)
                ));
        using var writer = new StreamWriter(HttpContext.OpenResponseStream());
        await writer.WriteAsync(doc.ToString());
    }
}