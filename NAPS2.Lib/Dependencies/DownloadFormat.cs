using System.IO.Compression;

namespace NAPS2.Dependencies;

public abstract class DownloadFormat
{
    public static readonly DownloadFormat Gzip = new GzipDownloadFormat();

    public static readonly DownloadFormat Zip = new ZipDownloadFormat();

    public abstract string Prepare(MemoryStream stream, string tempFilePath);

    private class GzipDownloadFormat : DownloadFormat
    {
        private const string FileExtension = ".gz";

        public override string Prepare(MemoryStream stream, string tempFilePath)
        {
            if (tempFilePath.EndsWith(FileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                tempFilePath = tempFilePath.Substring(0, tempFilePath.Length - 3);
            }
            Extract(stream, tempFilePath);
            return tempFilePath;
        }

        private static void Extract(MemoryStream stream, string destPath)
        {
            using FileStream outFile = File.Create(destPath);
            using GZipStream decompress = new(stream, CompressionMode.Decompress);
            decompress.CopyTo(outFile);
        }
    }

    private class ZipDownloadFormat : DownloadFormat
    {
        public override string Prepare(MemoryStream stream, string tempFilePath)
        {
            var tempDir = Path.GetDirectoryName(tempFilePath) ?? throw new ArgumentException("Path was a root path", nameof(tempFilePath));
            ZipArchive archive = new(stream);
            archive.ExtractToDirectory(tempDir);
            return tempDir;
        }
    }
}