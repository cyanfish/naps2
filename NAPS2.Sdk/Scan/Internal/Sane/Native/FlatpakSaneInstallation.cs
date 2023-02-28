using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using NAPS2.Platform.Linux;

namespace NAPS2.Scan.Internal.Sane.Native;

public class FlatpakSaneInstallation : ISaneInstallation
{
    private const string EXTERNAL_PREFIX = "/var/run/host";

    private readonly ScanningContext _scanningContext;

    public FlatpakSaneInstallation(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public bool CanStreamDevices => false;

    // TODO: We might want to make this a singleton or have some kind of static already-initialized state
    public void Initialize()
    {
        Log.Info($"ApplicationData: {Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}");
        Log.Info($"XDG_CONFIG_HOME: {Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")}");
        Log.Info($"TempFolderPath: {_scanningContext.TempFolderPath}");

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

        Log.Info($"Found external SANE config path: {externalConfigPath}");
        Log.Info($"Found internal SANE config path: {internalConfigPath}");

        var externalDllConf = ReadDllConf(externalConfigPath);
        var internalDllConf = ReadDllConf(internalConfigPath);
        if (externalDllConf == null || internalDllConf == null)
        {
            return;
        }
        
        var tempLibFolder = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
        // TODO: Better error handling
        Directory.CreateDirectory(tempLibFolder);
        foreach (var file in new DirectoryInfo(internalBackendsPath).EnumerateFiles())
        {
            file.CopyTo(Path.Combine(tempLibFolder, file.Name));
        }

        var backendsToCopy = externalDllConf
            .Where(x => x.IsEnabled)
            .Select(x => x.BackendName)
            .Except(internalDllConf.Select(x => x.BackendName));
        
        Log.Error($"Backends to copy: {string.Join(", ", backendsToCopy)}");
        Log.Error($"External backends: {string.Join(", ", externalDllConf.Select(x => x.BackendName))}");
        Log.Error($"Internal backends: {string.Join(", ", internalDllConf.Select(x => x.BackendName))}");

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
                var destPath = Path.Combine(tempLibFolder, fileName);
                try
                {
                    var linkBuf = new byte[1024];
                    int bytesWritten = LinuxInterop.readlink(sourcePath, linkBuf, linkBuf.Length);
                    if (bytesWritten < 0)
                    {
                        // Not a symlink
                        File.Copy(sourcePath, destPath, true);
                        Log.Info($"Copied sane backend {backendToCopy}");
                    }
                    else
                    {
                        // Adjust symlink path
                        var link = Encoding.UTF8.GetString(linkBuf, 0, bytesWritten);
                        var newLink = WithPrefix(link, EXTERNAL_PREFIX);
                        int linkResult = LinuxInterop.symlink(newLink, destPath);
                        Log.Info(
                            $"Copied sane backend {backendToCopy} with symlink {destPath} to {newLink} with code {linkResult} {Marshal.GetLastWin32Error()}");
                    }

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
        // TODO: Test unicode in this path
        var tempConfigFolder = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
        var tempConfigPath = Path.Combine(tempConfigFolder, "dll.conf");
        try
        {
            Directory.CreateDirectory(tempConfigFolder);
            // TODO: Is this the right logic for which backends should be enabled?
            var enabledBackends = internalDllConf
                .Where(x => x.IsEnabled)
                .Select(x => x.BackendName)
                .Concat(copiedBackends)
                .OrderBy(x => x);
            File.WriteAllLines(tempConfigPath, enabledBackends);
            Log.Info($"Setting sane lib dir to {tempLibFolder}");
            Log.Info($"Setting sane config dir to {tempConfigFolder}");
            PlatformCompat.System.SetEnv("LD_LIBRARY_PATH", tempLibFolder);
            PlatformCompat.System.SetEnv("SANE_CONFIG_DIR", tempConfigFolder);
            PlatformCompat.System.SetEnv("SANE_DEBUG_DLL", "255");
            PlatformCompat.System.SetEnv("SANE_DEBUG_ESCL", "255");
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error creating temporary sane config", ex);
        }

        try
        {
            foreach (var configFile in new DirectoryInfo(internalConfigPath)
                         .EnumerateFiles("*.conf")
                         .Where(x => x.Name != "dll.conf"))
            {
                File.Copy(
                    configFile.FullName,
                    Path.Combine(tempConfigFolder, configFile.Name));
            }

            // External (user-provided) config should overwrite internal (default) config
            // There is a potential issue here if there's a version mismatch that affects the config format.
            foreach (var configFile in new DirectoryInfo(externalConfigPath)
                         .EnumerateFiles("*.conf")
                         .Where(x => x.Name != "dll.conf"))
            {
                File.Copy(
                    configFile.FullName,
                    Path.Combine(tempConfigFolder, configFile.Name),
                    true);
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error copying additional sane config files", ex);
        }

        // TODO: Also copy other config files
    }

    private List<DllConfEntry>? ReadDllConf(string configPath)
    {
        try
        {
            var entries = new List<DllConfEntry>();
            var dllConfPaths = new List<string> { Path.Combine(configPath, "dll.conf") };
            try
            {
                var dllD = new DirectoryInfo(Path.Combine(configPath, "dll.d"));
                if (dllD.Exists)
                {
                    foreach (var file in dllD.EnumerateFiles())
                    {
                        dllConfPaths.Add(file.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error enumerating sane dll.d conf files", ex);
            }

            foreach (var dllConfPath in dllConfPaths)
            {
                foreach (var line in File.ReadLines(dllConfPath))
                {
                    var enabledMatch = Regex.Match(line.Trim(), @"^\w+$");
                    var disabledMatch = Regex.Match(line.Trim(), @"^#\w+$");
                    if (enabledMatch.Success)
                    {
                        entries.Add(new DllConfEntry(enabledMatch.Value, true));
                    }
                    else if (disabledMatch.Success)
                    {
                        entries.Add(new DllConfEntry(disabledMatch.Value.Substring(1), false));
                    }
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
            WithPrefix("/usr/etc/sane.d/", prefix),
            WithPrefix("/app/etc/sane.d/", prefix)
        };
        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                return searchPath;
            }
            else
            {
                Log.Error($"Dir does not exist: {searchPath} with prefix {prefix}");
            }
        }

        Log.Error($"Could not find sane config with prefix '{prefix}'");
        return null;
    }

    private string? FindSaneBackendsPath(string? prefix)
    {
        var searchPaths = ParseLdPaths(WithPrefix("/etc/ld.so.conf", prefix), prefix);
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

    private IEnumerable<string> ParseLdPaths(string prefixedPath, string? prefix)
    {
        if (!File.Exists(prefixedPath))
        {
            Log.Error($"Ld skipping nonexistent {prefixedPath}");
            yield break;
        }

        Log.Info($"Ld loading {prefixedPath}");
        foreach (var line in File.ReadLines(prefixedPath))
        {
            if (line.StartsWith("/"))
            {
                Log.Info($"Ld returning {line.Trim()}");
                yield return WithPrefix(line.Trim(), prefix);
            }

            if (line.StartsWith("include "))
            {
                var includedPathPattern = line.Substring(8).Trim();
                var lastDirIndex = includedPathPattern.LastIndexOf('/');
                var includedPathFolder = WithPrefix(includedPathPattern.Substring(0, lastDirIndex), prefix);
                var includedPathFilePattern = includedPathPattern.Substring(lastDirIndex + 1);
                IEnumerable<FileInfo> files;
                try
                {
                    Log.Info($"Ld searching in {includedPathFolder} for {includedPathFilePattern}");
                    files = new DirectoryInfo(includedPathFolder).EnumerateFiles(includedPathFilePattern);
                }
                catch (Exception ex)
                {
                    Log.ErrorException(
                        $"Error enumerating lib paths for sane: {includedPathFolder} {includedPathFilePattern}", ex);
                    continue;
                }

                foreach (var file in files)
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
        return prefix == null ? path : Path.Combine(prefix, path.StartsWith("/") ? path.Substring(1) : path);
    }

    public string LibraryPath => "libsane.so.1";

    public string[]? LibraryDeps => null;

    private record DllConfEntry(string BackendName, bool IsEnabled);
}