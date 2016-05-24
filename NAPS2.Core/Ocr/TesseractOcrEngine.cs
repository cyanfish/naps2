using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NAPS2.Config;
using NAPS2.Util;

namespace NAPS2.Ocr
{
    public class TesseractOcrEngine : IOcrEngine
    {
        private const int DEFAULT_TIMEOUT = 60 * 1000;

        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly AppConfigManager appConfigManager;

        public TesseractOcrEngine(OcrDependencyManager ocrDependencyManager, AppConfigManager appConfigManager)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            this.appConfigManager = appConfigManager;
        }

        public bool CanProcess(string langCode)
        {
            if (string.IsNullOrEmpty(langCode) || ocrDependencyManager.InstalledTesseractExe == null)
            {
                return false;
            }
            // Support multiple specified languages (e.g. "eng+fra")
            return langCode.Split('+').All(code => ocrDependencyManager.InstalledTesseractLanguages.Any(x => x.Code == code));
        }

        public OcrResult ProcessImage(string imagePath, string langCode)
        {
            string tempHocrFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            string tempHocrFilePathWithExt = tempHocrFilePath + (ocrDependencyManager.HasNewTesseractExe ? ".hocr" : ".html");
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ocrDependencyManager.InstalledTesseractExe.Path,
                    Arguments = string.Format("\"{0}\" \"{1}\" -l {2} hocr", imagePath, tempHocrFilePath, langCode),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var tessdataParent = new FileInfo(ocrDependencyManager.InstalledTesseractExe.Path).Directory;
                if (tessdataParent == null)
                {
                    throw new InvalidOperationException();
                }
                var tessdata = new DirectoryInfo(Path.Combine(tessdataParent.FullName, "tessdata"));
                startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = tessdataParent.FullName;
                EnsureHocrConfigExists(tessdata);
                var tesseractProcess = Process.Start(startInfo);
                if (tesseractProcess == null)
                {
                    // Couldn't start tesseract for some reason
                    Log.Error("Couldn't start OCR process.");
                    return null;
                }
                var timeout = (int)(appConfigManager.Config.OcrTimeoutInSeconds * 1000);
                if (timeout == 0)
                {
                    timeout = DEFAULT_TIMEOUT;
                }
                if (!tesseractProcess.WaitForExit(timeout))
                {
                    Log.Error("OCR process timed out.");
                    try
                    {
                        tesseractProcess.Kill();
                        // Wait a bit to give the process time to release its file handles
                        Thread.Sleep(200);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorException("Error killing OCR process", e);
                    }
                    return null;
                }
#if DEBUG
                var output = tesseractProcess.StandardOutput.ReadToEnd();
                if (output.Length > 0)
                {
                    Log.Error("Tesseract stdout: {0}", output);
                }
                output = tesseractProcess.StandardError.ReadToEnd();
                if (output.Length > 0)
                {
                    Log.Error("Tesseract stderr: {0}", output);
                }
#endif
                XDocument hocrDocument = XDocument.Load(tempHocrFilePathWithExt);
                return new OcrResult
                {
                    PageBounds = hocrDocument.Descendants()
                        .Where(x => x.Attributes("class").Any(y => y.Value == "ocr_page"))
                        .Select(x => GetBounds(x.Attribute("title")))
                        .First(),
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
                try
                {
                    File.Delete(tempHocrFilePathWithExt);
                }
                catch (Exception e)
                {
                    Log.ErrorException("Error cleaning up OCR temp files", e);
                }
            }
        }

        private void EnsureHocrConfigExists(DirectoryInfo tessdata)
        {
            var configDir = new DirectoryInfo(Path.Combine(tessdata.FullName, "configs"));
            if (!configDir.Exists)
            {
                configDir.Create();
            }
            var hocrConfigFile = new FileInfo(Path.Combine(configDir.FullName, "hocr"));
            if (!hocrConfigFile.Exists)
            {
                using (var writer = hocrConfigFile.CreateText())
                {
                    writer.Write("tessedit_create_hocr 1");
                }
            }
        }

        private Rectangle GetBounds(XAttribute titleAttr)
        {
            var bounds = new Rectangle();
            if (titleAttr != null)
            {
                foreach (var param in titleAttr.Value.Split(';'))
                {
                    string[] parts = param.Trim().Split(' ');
                    if (parts.Length == 5 && parts[0] == "bbox")
                    {
                        int x1 = int.Parse(parts[1]), y1 = int.Parse(parts[2]);
                        int x2 = int.Parse(parts[3]), y2 = int.Parse(parts[4]);
                        bounds = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                    }
                }
            }
            return bounds;
        }
    }
}
