using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Platform;
using NAPS2.Images.Storage;
using NAPS2.Scan;
using NAPS2.Serialization;
using NAPS2.Util;
using NAPS2.WinForms;
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

            ProfileManager.Current = new ProfileManager("profiles.xml", Paths.AppData, Paths.Executable);
            UserConfig.Manager = new ConfigManager<UserConfig>("config.xml", Paths.AppData, Paths.Executable, UserConfig.Create, new DefaultSerializer<UserConfig>());
            AppConfig.Manager = new ConfigManager<AppConfig>("appsettings.xml", Paths.Executable, null, AppConfig.Create, new DefaultSerializer<AppConfig>());

            var customPath = AppConfig.Current.ComponentsPath;
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
