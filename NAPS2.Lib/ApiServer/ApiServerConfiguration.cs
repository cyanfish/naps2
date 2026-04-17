using System.Security.Cryptography.X509Certificates;
using NAPS2.Config;

namespace NAPS2.Lib.ApiServer;

/// <summary>
/// API 服务配置类，包含端口、HTTPS、CORS 等通用设置。
/// </summary>
public class ApiServerConfiguration
{
    /// <summary>
    /// 监听端口，默认 8080。
    /// </summary>
    public int Port { get; set; } = 8080;

    /// <summary>
    /// 是否启用 HTTPS。
    /// </summary>
    public bool EnableHttps { get; set; }

    /// <summary>
    /// 是否启用跨域。
    /// </summary>
    public bool EnableCors { get; set; } = true;

    /// <summary>
    /// 绑定地址，默认 localhost。
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// 可选的证书文件路径。
    /// </summary>
    public string? CertificatePath { get; set; }

    /// <summary>
    /// 可选的证书密码。
    /// </summary>
    public string? CertificatePassword { get; set; }

    /// <summary>
    /// 返回完整 URL 前缀，例如 http://localhost:8080/ 或 https://localhost:8443/。
    /// </summary>
    public string UrlPrefix => $"{(EnableHttps ? "https" : "http")}://{Host}:{Port}/";

    /// <summary>
    /// 验证配置是否合法。
    /// </summary>
    public void Validate()
    {
        if (Port < 1024 || Port > 65535)
        {
            throw new InvalidOperationException($"Port {Port} is invalid. Port must be between 1024 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException("Host must not be empty.");
        }

        if (EnableHttps && !string.IsNullOrWhiteSpace(CertificatePath) && !File.Exists(CertificatePath))
        {
            throw new FileNotFoundException($"Certificate file not found: {CertificatePath}", CertificatePath);
        }
    }

    /// <summary>
    /// 加载 X509 证书，如果启用了 HTTPS 并提供了路径。
    /// </summary>
    public X509Certificate2? LoadCertificate()
    {
        if (!EnableHttps)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(CertificatePath))
        {
            return null;
        }

        return string.IsNullOrEmpty(CertificatePassword)
            ? new X509Certificate2(CertificatePath)
            : new X509Certificate2(CertificatePath, CertificatePassword);
    }

    /// <summary>
    /// 从命令行参数中创建配置对象。
    /// </summary>
    public static ApiServerConfiguration CreateFromArgs(string[] args)
    {
        var config = new ApiServerConfiguration();

        foreach (var arg in args)
        {
            var pair = arg.Split('=', 2);
            var key = pair[0].TrimStart('-', '/').ToLowerInvariant();
            var value = pair.Length > 1 ? pair[1] : string.Empty;

            switch (key)
            {
                case "port":
                    config.Port = int.TryParse(value, out var p) ? p : config.Port;
                    break;
                case "host":
                    config.Host = string.IsNullOrWhiteSpace(value) ? config.Host : value;
                    break;
                case "https":
                case "enablehttps":
                    config.EnableHttps = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(value);
                    break;
                case "cors":
                case "enablecors":
                    config.EnableCors = string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ? false : true;
                    break;
                case "certificate":
                case "certificatepath":
                    config.CertificatePath = value;
                    break;
                case "certificatepassword":
                    config.CertificatePassword = value;
                    break;
                case "help":
                case "?":
                    PrintHelp();
                    break;
            }
        }

        return config;
    }

    public static ApiServerConfiguration CreateFromConfig(Naps2Config config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        return new ApiServerConfiguration
        {
            Port = config.Get(c => c.ApiServerPort),
            Host = string.IsNullOrWhiteSpace(config.Get(c => c.ApiServerHost)) ? "localhost" : config.Get(c => c.ApiServerHost),
            EnableHttps = config.Get(c => c.ApiServerEnableHttps),
            EnableCors = config.Get(c => c.ApiServerEnableCors)
        };
    }

    private static void PrintHelp()
    {
        Console.WriteLine("NAPS2.App.ApiServer 参数:");
        Console.WriteLine("  --port=<port>               API 服务监听端口，默认 8080");
        Console.WriteLine("  --host=<hostname>           绑定地址，默认 localhost");
        Console.WriteLine("  --https[=true]              启用 HTTPS");
        Console.WriteLine("  --cors[=true|false]         启用/禁用 CORS，默认启用");
        Console.WriteLine("  --certificate=<path>        HTTPS 证书路径");
        Console.WriteLine("  --certificatepassword=<pwd>  HTTPS 证书密码");
        Console.WriteLine("  --help                      显示帮助");
        Environment.Exit(0);
    }
}
