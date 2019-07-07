using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Images.Storage;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Platform;
using Ninject;
using NLog;

namespace NAPS2
{
    public static class StaticConfiguration
    {
        public static void Initialize(IKernel kernel)
        {
            var configProvider = kernel.Get<ConfigScopes>().Provider;

            Log.Logger = new NLogLogger();
            if (PlatformCompat.System.CanUseWin32)
            {
                Log.EventLogger = new WindowsEventLogger(configProvider);
            }
#if DEBUG
            Debug.Listeners.Add(new NLogTraceListener());
#endif

            var customPath = configProvider.Get(c => c.ComponentsPath);
            var basePath = string.IsNullOrWhiteSpace(customPath)
                ? Paths.Components
                : Environment.ExpandEnvironmentVariables(customPath);

            OcrEngineManager.Default = new OcrEngineManager(basePath);

            var imageContext = kernel.Get<ImageContext>();
            var recoveryFolderPath = Path.Combine(Paths.Recovery, Path.GetRandomFileName());
            var rsm = new RecoveryStorageManager(recoveryFolderPath);
            imageContext.FileStorageManager = rsm;
            imageContext.ConfigureBackingStorage<FileStorage>();
            imageContext.ImageMetadataFactory = rsm;
        }
    }
}
