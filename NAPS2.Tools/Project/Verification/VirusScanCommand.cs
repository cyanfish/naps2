using NAPS2.Tools.Project.Targets;
using VirusTotalNet;
using VirusTotalNet.ResponseCodes;

namespace NAPS2.Tools.Project.Verification;

public class VirusScanCommand : ICommand<VirusScanOptions>
{
    public int Run(VirusScanOptions opts)
    {
        Output.Info("Checking for antivirus false positives");
        var version = ProjectHelper.GetCurrentVersionName();

        var tasks = new List<Task>();
        foreach (var target in TargetsHelper.EnumeratePackageTargets(opts.PackageType, opts.Platform, false))
        {
            var ext = target.Type.ToString().ToLowerInvariant();
            tasks.Add(StartVirusScan(ProjectHelper.GetPackagePath(ext, target.Platform, version)));
        }
        Task.WaitAll(tasks.ToArray());
        Output.OperationEnd("No antivirus false positives.");
        return 0;
    }

    private static async Task StartVirusScan(string packagePath)
    {
        var key = await File.ReadAllTextAsync(Path.Combine(Paths.Naps2UserFolder, "virustotal"));
        VirusTotal virusTotal = new VirusTotal(key.Trim())
        {
            UseTLS = true
        };

        var file = new FileInfo(packagePath);
        var report = await virusTotal.GetFileReportAsync(file);
        if (report.ResponseCode == FileReportResponseCode.NotPresent)
        {
            Output.Verbose($"Uploading to VirusTotal: {packagePath}");
            await virusTotal.ScanFileAsync(await File.ReadAllBytesAsync(packagePath), Path.GetFileName(packagePath));
        }
        else
        {
            Output.Verbose(report.ResponseCode == FileReportResponseCode.Queued
                ? $"VirusTotal already has a report queued for: {packagePath}"
                : $"VirusTotal already has a report completed for: {packagePath}");
        }
        Output.Info($"Report permalink: {report.Permalink}");
        while (report.ResponseCode != FileReportResponseCode.Present)
        {
            await Task.Delay(15000);
            report = await virusTotal.GetFileReportAsync(file);
        }
        if (report.Positives > 0)
        {
            throw new Exception($"VirusTotal has {report.Positives} engines with positive flags. {report.Permalink}");
        }
        Output.Info($"No false positives.");
    }
}