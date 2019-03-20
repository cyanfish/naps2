using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Config.Experimental;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Platform;
using NAPS2.Serialization;
using NLog;

namespace NAPS2
{
    public static class StaticConfiguration
    {
        public static void Initialize()
        {
            Log.Logger = new NLogLogger();
            if (PlatformCompat.System.CanUseWin32)
            {
                Log.EventLogger = new WindowsEventLogger();
            }
#if DEBUG
            Debug.Listeners.Add(new NLogTraceListener());
#endif

            ConfigScopes.Current = new ConfigScopes(Path.Combine(Paths.Executable, "appsettings.xml"), Path.Combine(Paths.AppData, "config.xml"));
            var configProvider = ConfigScopes.Current.Provider;
            ProfileManager.Current = new ProfileManager(
                Path.Combine(Paths.AppData, "profiles.xml"),
                Path.Combine(Paths.Executable, "profiles.xml"),
                configProvider.Get(c => c.LockSystemProfiles),
                configProvider.Get(c => c.LockUnspecifiedDevices),
                configProvider.Get(c => c.NoUserProfiles));

            var customPath = configProvider.Get(c => c.ComponentsPath);
            var basePath = string.IsNullOrWhiteSpace(customPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customPath);

            GhostscriptManager.BasePath = basePath;
            OcrEngineManager.Default = new OcrEngineManager(basePath);

            var recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
            var rsm = new RecoveryStorageManager(recoveryFolderPath);
            FileStorageManager.Current = rsm;
            StorageManager.ConfigureBackingStorage<FileStorage>();
            StorageManager.ImageMetadataFactory = rsm;
        }
    }
}
