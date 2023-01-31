using System.Text.RegularExpressions;

namespace NAPS2.Scan.Internal.Sane.Native;

public class FlatpakSaneInstallation : ISaneInstallation
{
    private const string EXTERNAL_PREFIX = "/var/run/host";

    private readonly ScanningContext _scanningContext;

    public FlatpakSaneInstallation(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    // TODO: We might want to make this a singleton or have some kind of static already-initialized state
    public void Initialize()
    {
        // TODO: Maybe cache these
        var externalBackendsPath = FindSaneBackendsPath(EXTERNAL_PREFIX);
        var internalBackendsPath = FindSaneBackendsPath(null);
        if (externalBackendsPath == null || internalBackendsPath == null)
        {
            return;
        }
        Log.Info($"Found external SANE backends path: {externalBackendsPath}");
        Log.Info($"Found internal SANE backends path: {internalBackendsPath}");

        var externalConfigPath = FindSaneConfigPath(EXTERNAL_PREFIX);
        var internalConfigPath = FindSaneConfigPath(null);
        if (externalConfigPath == null || internalConfigPath == null)
        {
            return;
        }
        Log.Info($"Found external SANE config path: {externalBackendsPath}");
        Log.Info($"Found internal SANE config path: {internalBackendsPath}");

        var externalDllConf = ReadDllConf(externalConfigPath);
        var internalDllConf = ReadDllConf(externalConfigPath);
        if (externalDllConf == null || internalDllConf == null)
        {
            return;
        }

        var backendsToCopy = externalDllConf
            .Where(x => x.IsEnabled)
            .Select(x => x.BackendName)
            .Except(internalDllConf.Select(x => x.BackendName));

        List<string> copiedBackends = new();

        foreach (var backendToCopy in backendsToCopy)
        {
            var fileName = $"libsane-{backendToCopy}.so.1";
            var sourcePath = Path.Combine(externalBackendsPath, fileName);
            if (!File.Exists(sourcePath))
            {
                Log.Error(
                    $"Sane config specified backend '{backendToCopy}' but it couldn't be found at '{sourcePath}'");
            }
            else
            {
                var destPath = Path.Combine(internalBackendsPath, fileName);
                try
                {
                    File.Copy(sourcePath, destPath, true);
                    copiedBackends.Add(backendToCopy);
                }
                catch (Exception ex)
                {
                    Log.ErrorException($"Error copying sane backend '{backendToCopy}'", ex);
                    if (File.Exists(destPath))
                    {
                        // Might have had an error but if the file is there (e.g. concurrent writing), use it
                        copiedBackends.Add(backendToCopy);
                    }
                }
            }
        }

        // TODO: Make this IDisposable and clean up after
        var tempConfigFolder = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
        var tempConfigPath = Path.Combine(tempConfigFolder, "dll.conf");
        try
        {
            Directory.CreateDirectory(tempConfigFolder);
            var enabledBackends = internalDllConf
                .Where(x => x.IsEnabled)
                .Select(x => x.BackendName)
                .Concat(copiedBackends);
            File.WriteAllLines(tempConfigPath, enabledBackends);
            PlatformCompat.System.SetEnv("SANE_CONFIG_DIR", tempConfigFolder);
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error creating temporary sane config", ex);
        }

        // TODO: Also copy other config files
    }

    private List<DllConfEntry>? ReadDllConf(string configPath)
    {
        try
        {
            var entries = new List<DllConfEntry>();
            foreach (var line in File.ReadLines(configPath))
            {
                var enabledMatch = Regex.Match(line.Trim(), @"\w+");
                var disabledMatch = Regex.Match(line.Trim(), @"#\w+");
                if (enabledMatch.Success)
                {
                    entries.Add(new DllConfEntry(enabledMatch.Value, true));
                }
                else if (disabledMatch.Success)
                {
                    entries.Add(new DllConfEntry(disabledMatch.Value.Substring(1), false));
                }
            }
            return entries;
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error reading sane dll.conf", ex);
            return null;
        }
    }

    private string? FindSaneConfigPath(string? prefix)
    {
        var searchPaths = new[]
        {
            WithPrefix("/etc/sane.d/", prefix),
            WithPrefix("/usr/etc/sane.d/", prefix)
        };
        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                return searchPath;
            }
        }
        Log.Error($"Could not find sane config with prefix '{prefix}'");
        return null;
    }

    private string? FindSaneBackendsPath(string? prefix)
    {
        var searchPaths = ParseLdPaths("/etc/ld.so.conf", prefix);
        foreach (var searchPath in searchPaths)
        {
            var backendsPath = Path.Combine(searchPath, "sane");
            if (Directory.Exists(backendsPath))
            {
                return backendsPath;
            }
        }
        Log.Error($"Could not find sane backends with prefix '{prefix}'");
        return null;
    }

    private IEnumerable<string> ParseLdPaths(string path, string? prefix)
    {
        var prefixedPath = WithPrefix(path, prefix);
        if (!File.Exists(prefixedPath)) yield break;
        foreach (var line in File.ReadLines(prefixedPath))
        {
            if (line.StartsWith("/"))
            {
                yield return line.Trim();
            }
            if (line.StartsWith("include "))
            {
                var includedPathPattern = line.Substring(8).Trim();
                var lastDirIndex = includedPathPattern.LastIndexOf('/');
                var includedPathFolder = includedPathPattern.Substring(0, lastDirIndex);
                var includedPathFilePattern = includedPathPattern.Substring(lastDirIndex + 1);
                foreach (var file in new DirectoryInfo(includedPathFolder).EnumerateFiles(includedPathFilePattern))
                {
                    foreach (var includedPath in ParseLdPaths(file.FullName, prefix))
                    {
                        yield return includedPath;
                    }
                }
            }
        }
    }

    private static string WithPrefix(string path, string? prefix)
    {
        return prefix == null ? path : Path.Combine(prefix, path);
    }

    public string LibraryPath => "libsane.1.so";

    public string[]? LibraryDeps => null;

    private record DllConfEntry(string BackendName, bool IsEnabled);
}