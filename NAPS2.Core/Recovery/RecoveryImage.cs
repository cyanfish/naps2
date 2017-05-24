using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Scan;
using NAPS2.Scan.Images;
using NAPS2.Scan.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Recovery
{
    public class RecoveryImage : IDisposable
    {
        public const string LOCK_FILE_NAME = ".lock";

        private static DirectoryInfo _recoveryFolder;
        private static FileInfo _recoveryLockFile;
        private static FileStream _recoveryLock;
        private static RecoveryIndexManager _recoveryIndexManager;

        private static int _recoveryFileNumber = 1;

        public static bool DisableRecoveryCleanup { get; set; }

        public static DirectoryInfo RecoveryFolder
        {
            get
            {
                if (_recoveryFolder == null)
                {
                    // Automatically create a recovery folder owned by this process
                    _recoveryFolder = new DirectoryInfo(Path.Combine(Paths.Recovery, Path.GetRandomFileName()));
                    _recoveryFolder.Create();
                    _recoveryLockFile = new FileInfo(Path.Combine(_recoveryFolder.FullName, LOCK_FILE_NAME));
                    _recoveryLock = _recoveryLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    _recoveryIndexManager = new RecoveryIndexManager(_recoveryFolder);
                }
                return _recoveryFolder;
            }
            set
            {
                if (_recoveryIndexManager != null)
                {
                    // Oops, this process already owns a recovery folder
                    throw new InvalidOperationException();
                }
                // If _recoveryFolder is set like this then this process doesn't own it, so we can't modify the index or lock
                _recoveryFolder = value;
            }
        }

        public static int RecoveryFileNumber
        {
            get => _recoveryFileNumber;
            set => _recoveryFileNumber = value;
        }

        public static RecoveryImage CreateNew(ImageFormat fileFormat, ScanBitDepth bitDepth, bool highQuality, List<Transform> transformList)
        {
            return new RecoveryImage(fileFormat, bitDepth, highQuality, transformList);
        }

        public static RecoveryImage LoadExisting(RecoveryIndexImage recoveryIndexImage)
        {
            return new RecoveryImage(recoveryIndexImage);
        }

        public static void Refresh(IEnumerable<ScannedImage> images)
        {
            if (_recoveryIndexManager != null)
            {
                _recoveryIndexManager.Index.Images.RemoveAll();
                _recoveryIndexManager.Index.Images.AddRange(images.Select(x => x.RecoveryIndexImage));
                _recoveryIndexManager.Save();
            }
        }

        private static string GetExtension(ImageFormat imageFormat)
        {
            if (ReferenceEquals(imageFormat, null))
            {
                return ".pdf";
            }
            if (Equals(imageFormat, ImageFormat.Png))
            {
                return ".png";
            }
            if (Equals(imageFormat, ImageFormat.Jpeg))
            {
                return ".jpg";
            }
            throw new ArgumentException();
        }

        private bool saved;
        private bool disposed;

        private RecoveryImage(ImageFormat fileFormat, ScanBitDepth bitDepth, bool highQuality, List<Transform> transformList)
        {
            FileFormat = fileFormat;
            FileName = (_recoveryFileNumber++).ToString("D5", CultureInfo.InvariantCulture) + GetExtension(FileFormat);
            FilePath = Path.Combine(RecoveryFolder.FullName, FileName);
            IndexImage = new RecoveryIndexImage
            {
                FileName = FileName,
                BitDepth = bitDepth,
                HighQuality = highQuality,
                TransformList = transformList
            };
        }

        private RecoveryImage(RecoveryIndexImage recoveryIndexImage)
        {
            if (_recoveryIndexManager.Index.Images.Contains(recoveryIndexImage))
            {
                throw new ArgumentException("Recovery image already exists in index");
            }

            string ext = Path.GetExtension(recoveryIndexImage.FileName);
            FileFormat = ".png".Equals(ext, StringComparison.InvariantCultureIgnoreCase) ? ImageFormat.Png : ImageFormat.Jpeg;
            FileName = recoveryIndexImage.FileName;
            _recoveryFileNumber++;
            FilePath = Path.Combine(RecoveryFolder.FullName, FileName);
            IndexImage = recoveryIndexImage;
            Save();
        }

        public ImageFormat FileFormat { get; }

        public string FileName { get; }

        public string FilePath { get; }

        public RecoveryIndexImage IndexImage { get; }

        public void Save()
        {
            if (disposed)
            {
                throw new InvalidOperationException();
            }
            if (_recoveryIndexManager != null)
            {
                if (!saved)
                {
                    _recoveryIndexManager.Index.Images.Add(IndexImage);
                    saved = true;
                }
                _recoveryIndexManager.Save();
            }
        }

        public void Move(int index)
        {
            if (!saved || disposed || _recoveryIndexManager == null)
            {
                throw new InvalidOperationException();
            }
            _recoveryIndexManager.Index.Images.Remove(IndexImage);
            _recoveryIndexManager.Index.Images.Insert(index, IndexImage);
            _recoveryIndexManager.Save();
        }

        public void Dispose()
        {
            try
            {
                if (_recoveryIndexManager != null && !DisableRecoveryCleanup && File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                    _recoveryIndexManager.Index.Images.Remove(IndexImage);
                    _recoveryIndexManager.Save();
                    if (_recoveryIndexManager.Index.Images.Count == 0)
                    {
                        _recoveryLock.Dispose();
                        RecoveryFolder.Delete(true);
                        _recoveryFolder = null;
                    }
                }
            }
            catch (IOException ex)
            {
                Log.ErrorException("Error cleaning up recovery files.", ex);
            }
            disposed = true;
        }
    }
}
