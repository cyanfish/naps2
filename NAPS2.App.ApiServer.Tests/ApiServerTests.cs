using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using NAPS2.App.ApiServer;
using NAPS2.Lib.ApiServer;
using Xunit;

namespace NAPS2.App.ApiServer.Tests;

public class ApiServerTests : IDisposable
{
    private readonly ApiServer _server;
    private readonly int _port;

    public ApiServerTests()
    {
        _port = GetAvailablePort();
        _server = new ApiServer(new ApiServerConfiguration
        {
            Port = _port,
            Host = "localhost",
            EnableCors = false
        });
    }

    [Fact]
    public async Task StartAsync_ThenHealthEndpoint_ReturnsHealthy()
    {
        await _server.StartAsync();

        using var client = new HttpClient();
        var response = await client.GetAsync($"http://localhost:{_port}/api/health");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();

        Assert.NotNull(payload);
        Assert.True(payload.ContainsKey("healthy"));
        Assert.True(payload["healthy"].GetBoolean());
    }

    [Fact]
    public void Validate_InvalidPort_ThrowsArgumentException()
    {
        var config = new ApiServerConfiguration
        {
            Port = 80,
            Host = "localhost"
        };

        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    public void Dispose()
    {
        _server.StopAsync().GetAwaiter().GetResult();
        _server.Dispose();
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
