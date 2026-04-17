using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NAPS2.Lib.ApiServer;

namespace NAPS2.App.ApiServer.Controllers;

public class SettingsController : WebApiController
{
    private readonly ApiServerConfiguration _config;

    public SettingsController(ApiServerConfiguration config)
    {
        _config = config;
    }

    [Route(HttpVerbs.Get, "/settings")]
    public object GetSettings()
    {
        return new
        {
            port = _config.Port,
            enableHttps = _config.EnableHttps,
            enableCors = _config.EnableCors,
            host = _config.Host,
            urlPrefix = _config.UrlPrefix
        };
    }
}
