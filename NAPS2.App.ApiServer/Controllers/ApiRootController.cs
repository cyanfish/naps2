using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NAPS2.Lib.ApiServer;

namespace NAPS2.App.ApiServer.Controllers;

public class ApiRootController : WebApiController
{
    private readonly ApiServerConfiguration _config;

    public ApiRootController(ApiServerConfiguration config)
    {
        _config = config;
    }

    [Route(HttpVerbs.Get, "/status")]
    public object GetStatus()
    {
        return new
        {
            status = "running",
            version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown",
            port = _config.Port,
            urlPrefix = _config.UrlPrefix,
            https = _config.EnableHttps,
            cors = _config.EnableCors
        };
    }

    [Route(HttpVerbs.Get, "/health")]
    public object GetHealth()
    {
        return new
        {
            healthy = true,
            timestamp = DateTime.UtcNow
        };
    }

    [Route(HttpVerbs.Get, "/version")]
    public object GetVersion()
    {
        return new
        {
            version = GetType().Assembly.GetName().Version?.ToString() ?? "unknown"
        };
    }
}
