using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NAPS2.Util
{
    public abstract class DownloadFormat
    {
        public static DownloadFormat Gzip = new GzipDownloadFormat();

        public abstract string Prepare(string tempFilePath);

        private class GzipDownloadFormat : DownloadFormat
        {
            public override string Prepare(string tempFilePath)
            {
                if (!tempFilePath.EndsWith(".gz"))
                {
                    throw new ArgumentException();
                }
                var pathWithoutGz = tempFilePath.Substring(0, tempFilePath.Length - 3);
                Extract(tempFilePath, pathWithoutGz);
                return pathWithoutGz;
            }

            private static void Extract(string sourcePath, string destPath)
            {
                using (FileStream inFile = new FileInfo(sourcePath).OpenRead())
                {
                    using (FileStream outFile = File.Create(destPath))
                    {
                        using (GZipStream decompress = new GZipStream(inFile, CompressionMode.Decompress))
                        {
                            decompress.CopyTo(outFile);
                        }
                    }
                }
            }
        }
    }
}
