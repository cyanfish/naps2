using NAPS2.Tools.Project.Targets;
using VirusTotalNet;
using VirusTotalNet.ResponseCodes;

namespace NAPS2.Tools.Project.Verification;

public static class VirusScanCommand
{
    public static int Run(VirusScanOptions opts)
    {
        Console.WriteLine("Checking for antivirus false positives");
        var version = ProjectHelper.GetDefaultProjectVersion();

        using var appDriverRunner = AppDriverRunner.Start(opts.Verbose);

        var constraints = new TargetConstraints
        {
            AllowMultiplePlatforms = true
        };
        var tasks = new List<Task>();
        foreach (var target in TargetsHelper.Enumerate(opts.BuildType, opts.Platform, constraints))
        {
            switch (target.BuildType)
            {
                case BuildType.Exe:
                    tasks.Add(StartVirusScan(ProjectHelper.GetPackagePath("exe", target.Platform, version),
                        opts.Verbose));
                    break;
                case BuildType.Msi:
                    tasks.Add(StartVirusScan(ProjectHelper.GetPackagePath("msi", target.Platform, version),
                        opts.Verbose));
                    break;
                case BuildType.Zip:
                    tasks.Add(StartVirusScan(ProjectHelper.GetPackagePath("zip", target.Platform, version),
                        opts.Verbose));
                    break;
            }
        }
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine(opts.Verbose ? "No antivirus false positives." : "Done.");
        return 0;
    }

    private static async Task StartVirusScan(string packagePath, bool verbose)
    {
        var key = await File.ReadAllTextAsync(Path.Combine(Paths.Naps2UserFolder, "virustotal"));
        VirusTotal virusTotal = new VirusTotal(key)
        {
            UseTLS = true
        };

        var file = new FileInfo(packagePath);
        var report = await virusTotal.GetFileReportAsync(file);
        if (report.ResponseCode == FileReportResponseCode.NotPresent)
        {
            if (verbose)
            {
                Console.WriteLine($"Uploading to VirusTotal: {packagePath}");
            }
            await virusTotal.ScanFileAsync(await File.ReadAllBytesAsync(packagePath), Path.GetFileName(packagePath));
        }
        else if (verbose)
        {
            Console.WriteLine(report.ResponseCode == FileReportResponseCode.Queued
                ? $"VirusTotal already has a report queued for: {packagePath}"
                : $"VirusTotal already has a report completed for: {packagePath}");
        }
        Console.WriteLine($"Report permalink: {report.Permalink}");
        while (report.ResponseCode != FileReportResponseCode.Present)
        {
            await Task.Delay(15000);
            report = await virusTotal.GetFileReportAsync(file);
        }
        if (report.Positives > 0)
        {
            throw new Exception($"VirusTotal has {report.Positives} engines with positive flags. {report.Permalink}");
        }
        Console.WriteLine($"No false positives.");
    }
}