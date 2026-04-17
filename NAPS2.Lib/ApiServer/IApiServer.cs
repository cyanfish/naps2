using System.Threading;

namespace NAPS2.Lib.ApiServer;

/// <summary>
/// 定义 API 服务生命周期接口，供主程序和独立服务引用。
/// </summary>
public interface IApiServer : IDisposable
{
    /// <summary>
    /// 启动服务。
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止服务。
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务是否正在运行。
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// 当前服务监听端口。
    /// </summary>
    int Port { get; }
}
