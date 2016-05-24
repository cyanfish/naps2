using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NAPS2.Ocr
{
    public class OcrComponent
    {
        public static string BasePath { get; set; }

        public OcrComponent(string path)
        {
            Path = System.IO.Path.Combine(BasePath, path);
        }

        public string Path { get; private set; }

        public bool IsInstalled
        {
            get
            {
                if (Path == null)
                {
                    return false;
                }
                return File.Exists(Path);
            }
        }

        public void Install(string sourcePath)
        {
            if (Path == null)
            {
                throw new InvalidOperationException();
            }
            File.Move(sourcePath, Path);
        }
    }
}
