using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Google.Protobuf;
using NAPS2.Images;
using NAPS2.Images.Storage;
using NAPS2.Serialization;

namespace NAPS2.ImportExport
{
    [Serializable]
    public class DirectImageTransfer
    {
        public DirectImageTransfer(ImageContext imageContext, IEnumerable<ScannedImage> images)
        {
            ProcessID = Process.GetCurrentProcess().Id;
            var serializedImages = images.Select(x => SerializedImageHelper.Serialize(imageContext, x, new SerializedImageHelper.SerializeOptions()));
            SerializedImages = serializedImages.Select(x => x.ToByteArray()).ToList();
        }

        public int ProcessID { get; }

        public List<byte[]> SerializedImages { get; }
    }
}
