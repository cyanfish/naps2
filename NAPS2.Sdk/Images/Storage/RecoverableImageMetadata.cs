using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NAPS2.Images.Transforms;
using NAPS2.Recovery;
using NAPS2.Scan;

namespace NAPS2.Images.Storage
{
    public class RecoverableImageMetadata : IImageMetadata
    {
        private readonly RecoveryStorageManager rsm;
        private RecoveryIndexImage indexImage;

        public RecoverableImageMetadata(RecoveryStorageManager rsm, RecoveryIndexImage indexImage)
        {
            this.rsm = rsm;
            // TODO: Maybe not a constructor param?
            this.indexImage = indexImage;
        }

        public List<Transform> TransformList
        {
            get => indexImage.TransformList;
            set => indexImage.TransformList = value;
        }

        public int Index
        {
            get => rsm.Index.Images.IndexOf(indexImage);
            set
            {
                // TODO: Locking
                rsm.Index.Images.Remove(indexImage);
                rsm.Index.Images.Insert(value, indexImage);
            }
        }

        public ScanBitDepth BitDepth
        {
            get => indexImage.BitDepth;
            set => indexImage.BitDepth = value;
        }

        public bool Lossless
        {
            get => indexImage.HighQuality;
            set => indexImage.HighQuality = value;
        }

        public void Commit()
        {
            rsm.Commit();
        }

        public bool CanSerialize => true;

        public byte[] Serialize(IStorage storage)
        {
            var stream = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(SerializedMetadata), Transform.KnownTransformTypes);
            serializer.WriteObject(stream, new SerializedMetadata
            {
                RecoveryIndexImage = indexImage,
                RecoveryFolderPath = rsm.RecoveryFolderPath
            });
            return stream.ToArray();
        }

        public IStorage Deserialize(byte[] serializedData)
        {
            var serializer = new DataContractSerializer(typeof(SerializedMetadata), Transform.KnownTransformTypes);
            using (var stream = new MemoryStream(serializedData))
            {
                var data = (SerializedMetadata)serializer.ReadObject(stream);
                indexImage = data.RecoveryIndexImage;
                if (data.RecoveryFolderPath == rsm.RecoveryFolderPath)
                {
                    // Already exists in our recovery folder (i.e. it came from a worker scan)
                    return new FileStorage(Path.Combine(rsm.RecoveryFolderPath, indexImage.FileName));
                }
                else
                {
                    // Exists elsewhere (i.e. copying from another NAPS2 instance)
                    var oldPath = Path.Combine(data.RecoveryFolderPath, indexImage.FileName);
                    var ext = Path.GetExtension(indexImage.FileName);
                    var newPath = rsm.NextFilePath() + ext;
                    indexImage.FileName = Path.GetFileName(newPath);
                    File.Copy(oldPath, newPath);
                    if (ext.Equals(".pdf", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new PdfFileStorage(newPath);
                    }
                    else
                    {
                        return new FileStorage(newPath);
                    }
                }
            }
        }

        public void Dispose()
        {
            rsm.Index.Images.Remove(indexImage);
            // TODO: Commit?
        }

        private class SerializedMetadata
        {
            public RecoveryIndexImage RecoveryIndexImage { get; set; }

            public string RecoveryFolderPath { get; set; }
        }
    }
}
