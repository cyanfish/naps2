using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NAPS2.Ocr
{
    public class IndependentComponent
    {
        public IndependentComponent(string path)
        {
            Path = path;
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

    public class IndependentComponents
    {
        public readonly IndependentComponent Tesseract304Xp = new IndependentComponent(@"tesseract-3.0.4\tesseract_xp.exe");

        public readonly IndependentComponent Tesseract304 = new IndependentComponent(@"tesseract-3.0.4\tesseract.exe");

        public readonly IndependentComponent Tesseract302 = new IndependentComponent(@"tesseract-3.0.2\tesseract.exe");

        public readonly IndependentComponent Null = new IndependentComponent(null);
    }
}
