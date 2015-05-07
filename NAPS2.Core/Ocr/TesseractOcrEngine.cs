using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace NAPS2.Ocr
{
    public class TesseractOcrEngine : IOcrEngine
    {
        private const int TESSERACT_TIMEOUT_MS = 20 * 1000;
        private readonly OcrDependencyManager ocrDependencyManager;

        public TesseractOcrEngine(OcrDependencyManager ocrDependencyManager)
        {
            this.ocrDependencyManager = ocrDependencyManager;
        }

        public bool CanProcess(string langCode)
        {
            return ocrDependencyManager.IsExecutableDownloaded &&
                ocrDependencyManager.GetDownloadedLanguages().Any(x => x.Code == langCode);
        }

        public OcrResult ProcessImage(Image image, string langCode)
        {
            string tempImageFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            string tempHocrFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            try
            {
                image.Save(tempImageFilePath);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(ocrDependencyManager.GetExecutableDir().FullName, "tesseract.exe"),
                    Arguments = string.Format("\"{0}\" \"{1}\" hocr -l {2}", tempImageFilePath, tempHocrFilePath, langCode),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var tessdataParent = ocrDependencyManager.GetLanguageDir().Parent;
                if (tessdataParent != null)
                {
                    startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = tessdataParent.FullName;
                }
                EnsureHocrConfigExists();
                var tesseractProcess = Process.Start(startInfo);
                if (tesseractProcess == null)
                {
                    // Couldn't start tesseract for some reason
                    Log.Error("Couldn't start OCR process.");
                    return null;
                }
                if (!tesseractProcess.WaitForExit(TESSERACT_TIMEOUT_MS))
                {
                    Log.Error("OCR process timed out.");
                    try
                    {
                        tesseractProcess.Kill();
                    }
                    catch (Exception e)
                    {
                        Log.ErrorException("Error killing OCR process", e);
                    }
                    return null;
                }
                XDocument hocrDocument = XDocument.Load(tempHocrFilePath + ".html");
                return new OcrResult
                {
                    Elements = hocrDocument.Descendants()
                        .Where(x => x.Attributes("class").Any(y => y.Value == "ocrx_word"))
                        .Select(x => new OcrResultElement { Text = x.Value, Bounds = GetBounds(x.Attribute("title")) })
                };
            }
            catch (Exception e)
            {
                Log.ErrorException("Error running OCR", e);
                return null;
            }
            finally
            {
                File.Delete(tempImageFilePath);
                File.Delete(tempHocrFilePath + ".html");
            }
        }

        private void EnsureHocrConfigExists()
        {
            var tessdataDir = ocrDependencyManager.GetLanguageDir();
            var configDir = new DirectoryInfo(Path.Combine(tessdataDir.FullName, "configs"));
            if (!configDir.Exists)
            {
                configDir.Create();
            }
            var hocrConfigFile = new FileInfo(Path.Combine(configDir.FullName, "hocr"));
            using (var writer = hocrConfigFile.CreateText())
            {
                writer.Write("tessedit_create_hocr 1");
            }
        }

        private Rectangle GetBounds(XAttribute titleAttr)
        {
            var bounds = new Rectangle();
            if (titleAttr != null)
            {
                string[] parts = titleAttr.Value.Split(' ');
                if (parts.Length == 5 && parts[0] == "bbox")
                {
                    int x1 = int.Parse(parts[1]), y1 = int.Parse(parts[2]);
                    int x2 = int.Parse(parts[3]), y2 = int.Parse(parts[4]);
                    bounds = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                }
            }
            return bounds;
        }
    }
}
