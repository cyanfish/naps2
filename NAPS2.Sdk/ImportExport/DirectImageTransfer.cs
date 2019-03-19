using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Google.Protobuf;
using NAPS2.Images;
using NAPS2.Serialization;

namespace NAPS2.ImportExport
{
    [Serializable]
    public class DirectImageTransfer
    {
        public DirectImageTransfer(IEnumerable<ScannedImage> images)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            var serializedImages = images.Select(x => SerializedImageHelper.Serialize(x, new SerializedImageHelper.SerializeOptions()));
            SerializedImages = serializedImages.Select(x => x.ToByteArray()).ToList();
        }

        public DirectImageTransfer(IEnumerable<ScannedImage.Snapshot> snapshots)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            var serializedImages = snapshots.Select(x => SerializedImageHelper.Serialize(x, new SerializedImageHelper.SerializeOptions()));
            SerializedImages = serializedImages.Select(x => x.ToByteArray()).ToList();
        }

        public int ProcessID { get; }

        public List<byte[]> SerializedImages { get; }
    }
}
