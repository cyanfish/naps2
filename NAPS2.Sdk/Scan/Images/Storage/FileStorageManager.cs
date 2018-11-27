using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class FileStorageManager
    {
        public virtual string NextFilePath() => Path.Combine(Paths.Temp, Path.GetRandomFileName());

        public virtual void Attach(string path)
        {
            // TODO: Separate all this stuff out completely.
        }

        public virtual void Detach(string path)
        {
        }
    }
}
