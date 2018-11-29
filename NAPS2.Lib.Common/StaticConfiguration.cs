using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Platform;
using NAPS2.Scan.Images.Storage;
using NLog;

namespace NAPS2.DI
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

            UserConfig.Manager = new ConfigManager<UserConfig>("config.xml", Paths.AppData, Paths.Executable, UserConfig.Create);
            AppConfig.Manager = new ConfigManager<AppConfig>("appsettings.xml", Paths.Executable, null, AppConfig.Create);

            var customPath = AppConfig.Current.ComponentsPath;
            var basePath = string.IsNullOrWhiteSpace(customPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customPath);

            GhostscriptManager.BasePath = basePath;
            OcrManager.Default = new OcrManager(basePath);

            var recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
            var rsm = new RecoveryStorageManager(recoveryFolderPath);
            FileStorageManager.Default = rsm;
            StorageManager.ImageMetadataFactory = rsm;
        }
    }
}
