using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using NAPS2.Images;
using NAPS2.Lib.ApiServer;
using NAPS2.Scan;

namespace NAPS2.App.ApiServer.Controllers;

public class ScanController : WebApiController
{
    private readonly ApiServerConfiguration _config;
    private readonly ScanJobManager _jobManager;

    public ScanController(ApiServerConfiguration config, ScanJobManager jobManager)
    {
        _config = config;
        _jobManager = jobManager;
    }

    [Route(HttpVerbs.Get, "/scan/drivers")]
    public object GetDrivers()
    {
        return new
        {
            drivers = Enum.GetNames<Driver>()
        };
    }

    [Route(HttpVerbs.Get, "/scan/devices")]
    public async Task<object> GetDevices(string? driver = null)
    {
        var options = new ScanOptions();
        if (!string.IsNullOrWhiteSpace(driver) && Enum.TryParse<Driver>(driver, true, out var parsedDriver))
        {
            options.Driver = parsedDriver;
        }

        var devices = await _jobManager.GetDeviceListAsync(options);
        return new
        {
            driver = options.Driver,
            devices
        };
    }

    [Route(HttpVerbs.Get, "/scan/caps")]
    public async Task<object> GetCaps(string? driver = null, string? deviceId = null, string? deviceName = null)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(deviceName))
        {
            return new
            {
                success = false,
                message = "请提供 deviceId 和 deviceName。"
            };
        }

        var options = new ScanOptions
        {
            Device = new ScanDevice(ParseDriver(driver), deviceId, deviceName)
        };

        var caps = await _jobManager.GetCapsAsync(options);
        return new
        {
            success = true,
            caps
        };
    }

    [Route(HttpVerbs.Post, "/scan/start")]
    public async Task<object> StartScan()
    {
        var options = await ReadScanOptionsAsync();
        var job = _jobManager.CreateJob(options);
        return new
        {
            success = true,
            jobId = job.JobId,
            status = job.Status,
            startedAt = job.StartedAt,
            message = "Scan job started. Use /api/scan/jobs to monitor progress."
        };
    }

    [Route(HttpVerbs.Post, "/scan/cancel")]
    public object CancelScan()
    {
        var jobId = HttpContext.Request.QueryString["jobId"];
        if (string.IsNullOrWhiteSpace(jobId) || !_jobManager.TryGetJob(jobId, out var job))
        {
            return new
            {
                success = false,
                message = "请选择有效的作业 ID。"
            };
        }

        job.Cancel();
        return new
        {
            success = true,
            jobId,
            status = job.Status,
            message = "Scan job cancellation requested."
        };
    }

    [Route(HttpVerbs.Get, "/scan/jobs")]
    public object GetJobs()
    {
        return new
        {
            jobs = _jobManager.GetJobs().ToArray()
        };
    }

    [Route(HttpVerbs.Get, "/scan/jobs/{jobId}")]
    public object GetJob(string jobId)
    {
        if (!_jobManager.TryGetJob(jobId, out var job))
        {
            return new
            {
                success = false,
                message = "Scan job not found."
            };
        }

        return new
        {
            success = true,
            job = job.ToSummary(),
            device = job.Options.Device,
            driver = job.Options.Driver,
            options = job.Options
        };
    }

    [Route(HttpVerbs.Get, "/scan/jobs/{jobId}/export")]
    public async Task ExportJob(string jobId, string? format = "pdf", string? encoding = "base64", string? imageFormat = "png")
    {
        if (!_jobManager.TryGetJob(jobId, out var job))
        {
            Response.ContentType = "application/json";
            Response.StatusCode = 404;
            await WriteJsonAsync(new
            {
                success = false,
                message = "Scan job not found."
            });
            return;
        }

        if (!job.IsCompleted)
        {
            Response.ContentType = "application/json";
            Response.StatusCode = 400;
            await WriteJsonAsync(new
            {
                success = false,
                message = "Scan job is not complete yet."
            });
            return;
        }

        if (string.Equals(format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfData = await _jobManager.ExportJobPdfAsync(jobId);
            if (string.Equals(encoding, "binary", StringComparison.OrdinalIgnoreCase))
            {
                Response.ContentType = "application/pdf";
                Response.ContentLength64 = pdfData.Length;
                await HttpContext.OpenResponseStream().WriteAsync(pdfData);
                return;
            }

            Response.ContentType = "application/json";
            await WriteJsonAsync(new
            {
                success = true,
                contentType = "application/pdf",
                data = Convert.ToBase64String(pdfData)
            });
            return;
        }

        if (string.Equals(format, "image", StringComparison.OrdinalIgnoreCase))
        {
            var supportedImageFormat = ParseImageFormat(imageFormat);
            var images = job.Images;
            var result = new List<object>();
            foreach (var image in images)
            {
                using var rendered = image.Render();
                using var memoryStream = rendered.SaveToMemoryStream(supportedImageFormat);
                result.Add(new
                {
                    contentType = supportedImageFormat == ImageFileFormat.Png ? "image/png" : "image/jpeg",
                    data = Convert.ToBase64String(memoryStream.ToArray())
                });
            }

            Response.ContentType = "application/json";
            await WriteJsonAsync(new
            {
                success = true,
                format = "image",
                imageFormat = supportedImageFormat.ToString().ToLowerInvariant(),
                items = result
            });
            return;
        }

        Response.ContentType = "application/json";
        Response.StatusCode = 400;
        await WriteJsonAsync(new
        {
            success = false,
            message = "Unsupported export format. Use format=pdf or format=image."
        });
    }

    private static ImageFileFormat ParseImageFormat(string? imageFormat)
    {
        if (!string.IsNullOrWhiteSpace(imageFormat) && imageFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return ImageFileFormat.Jpeg;
        }

        return ImageFileFormat.Png;
    }

    private async Task WriteJsonAsync(object obj)
    {
        await using var stream = HttpContext.OpenResponseStream();
        await JsonSerializer.SerializeAsync(stream, obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static Driver ParseDriver(string? driver)
    {
        if (!string.IsNullOrWhiteSpace(driver) && Enum.TryParse<Driver>(driver, true, out var parsed))
        {
            return parsed;
        }

        return Driver.Default;
    }

    private async Task<ScanOptions> ReadScanOptionsAsync()
    {
        try
        {
            var stream = HttpContext.Request.InputStream;
            if (stream == null || !stream.CanRead)
            {
                return new ScanOptions();
            }

            var options = await JsonSerializer.DeserializeAsync<ScanOptions>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return options ?? new ScanOptions();
        }
        catch
        {
            return new ScanOptions();
        }
    }
}
