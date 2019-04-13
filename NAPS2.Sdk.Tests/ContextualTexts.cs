using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.ImportExport.Images;
using NAPS2.ImportExport.Pdf;
using NAPS2.Logging;
using NAPS2.Ocr;
using NAPS2.Operation;
using NAPS2.Util;
using NAPS2.WinForms;

namespace NAPS2.Sdk.Tests
{
    public class ContextualTexts : IDisposable
    {
        private RecoveryStorageManager rsm;

        public ContextualTexts()
        {
            FolderPath = $"naps2_test_temp_{Path.GetRandomFileName()}";
            Folder = Directory.CreateDirectory(FolderPath);
            var tempPath = Path.Combine(FolderPath, "temp");
            Directory.CreateDirectory(tempPath);
            FileStorageManager.Current = new FileStorageManager(tempPath);

            Log.Logger = new NullLogger();
            Log.EventLogger = new NullEventLogger();
            ProfileManager.Current = new StubProfileManager();

            var componentsPath = Path.Combine(FolderPath, "components");
            var ocrEngineManager = new OcrEngineManager(componentsPath);
            OcrEngineManager.Default = ocrEngineManager;

            ErrorOutput.Default = new StubErrorOutput();
            var operationProgress = new StubOperationProgress();
            OperationProgress.Default = operationProgress;
            OverwritePrompt.Default = new StubOverwritePrompt();
            DialogHelper.Default = new StubDialogHelper();
            BlankDetector.Default = new ThresholdBlankDetector();
            PdfExporter.Default = new PdfSharpExporter();
            OcrRequestQueue.Default = new OcrRequestQueue(ocrEngineManager, operationProgress);

            StorageManager.ConfigureImageType<GdiImage>();
            StorageManager.ConfigureBackingStorage<IStorage>();
            StorageManager.ImageMetadataFactory = new StubImageMetadataFactory();
        }

        public string FolderPath { get; }

        public DirectoryInfo Folder { get; }

        public void UseFileStorage()
        {
            StorageManager.ConfigureBackingStorage<FileStorage>();
        }

        public void UseRecovery()
        {
            var recoveryFolderPath = Path.Combine(FolderPath, "recovery", Path.GetRandomFileName());
            rsm = new RecoveryStorageManager(recoveryFolderPath);
            FileStorageManager.Current = rsm;
            StorageManager.ConfigureBackingStorage<FileStorage>();
            StorageManager.ImageMetadataFactory = rsm;
        }

        public virtual void Dispose()
        {
            rsm?.ForceReleaseLock();
            Directory.Delete(FolderPath, true);
        }
    }
}
