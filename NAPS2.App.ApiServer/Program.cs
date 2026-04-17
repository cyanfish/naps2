using NAPS2.App.ApiServer;
using NAPS2.Lib.ApiServer;

var config = ApiServerConfiguration.CreateFromArgs(args);
var server = new ApiServer(config);

Console.WriteLine($"Starting NAPS2 API Server on {config.UrlPrefix}");

try
{
    await server.StartAsync();
    Console.WriteLine("API server is running. Press Ctrl+C to stop.");
    var exit = new ManualResetEventSlim(false);
    Console.CancelKeyPress += (_, e) =>
    {
        e.Cancel = true;
        exit.Set();
    };

    exit.Wait();

    await server.StopAsync();
    Console.WriteLine("API server stopped.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"启动 API 服务失败：{ex.Message}");
    return 1;
}

return 0;
