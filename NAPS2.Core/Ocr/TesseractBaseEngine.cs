using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NAPS2.Config;
using NAPS2.Dependencies;
using NAPS2.Util;

namespace NAPS2.Ocr
{
    public abstract class TesseractBaseEngine : IOcrEngine
    {
        private const int DEFAULT_TIMEOUT = 120 * 1000;
        private const int CHECK_INTERVAL = 500;

        private readonly AppConfigManager appConfigManager;

        protected TesseractBaseEngine(AppConfigManager appConfigManager)
        {
            this.appConfigManager = appConfigManager;
        }

        public bool CanProcess(string langCode)
        {
            if (string.IsNullOrEmpty(langCode) || !IsInstalled || !IsSupported)
            {
                return false;
            }
            // Support multiple specified languages (e.g. "eng+fra")
            return langCode.Split('+').All(code => InstalledLanguages.Any(x => x.Code == code));
        }

        public OcrResult ProcessImage(string imagePath, OcrParams ocrParams, Func<bool> cancelCallback)
        {
            string tempHocrFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
            string tempHocrFilePathWithExt = tempHocrFilePath + TesseractHocrExtension;
            try
            {
                var runInfo = TesseractRunInfo(ocrParams);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(TesseractBasePath, TesseractExePath),
                    Arguments = $"\"{imagePath}\" \"{tempHocrFilePath}\" -l {ocrParams.LanguageCode} {runInfo.Arguments} hocr",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                if (runInfo.PrefixPath != null)
                {
                    startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = Path.Combine(TesseractBasePath, runInfo.PrefixPath);
                }
                if (runInfo.DataPath != null)
                {
                    var tessdata = new DirectoryInfo(Path.Combine(TesseractBasePath, runInfo.DataPath));
                    EnsureHocrConfigExists(tessdata);
                }
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
                var stopwatch = Stopwatch.StartNew();
                while (!tesseractProcess.WaitForExit(CHECK_INTERVAL))
                {
                    if (stopwatch.ElapsedMilliseconds >= timeout || cancelCallback())
                    {
                        if (stopwatch.ElapsedMilliseconds >= timeout)
                        {
                            Log.Error("OCR process timed out.");
                        }
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
                }
#if DEBUG
                Debug.WriteLine("Tesseract stopwatch: " + stopwatch.ElapsedMilliseconds);
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
                        .Select(x => new OcrResultElement { Text = x.Value, Bounds = GetBounds(x.Attribute("title")) }),
                    RightToLeft = InstalledLanguages.Where(x => x.Code == ocrParams.LanguageCode).Select(x => x.RTL).FirstOrDefault()
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

        protected abstract string TesseractBasePath { get; }

        protected abstract string TesseractExePath { get; }

        protected virtual string TesseractHocrExtension => ".hocr";

        protected virtual RunInfo TesseractRunInfo(OcrParams ocrParams) => new RunInfo
        {
            Arguments = "",
            DataPath = "tessdata",
            PrefixPath = ""
        };

        protected virtual DownloadInfo DownloadInfo => null;

        protected abstract PlatformSupport PlatformSupport { get; }

        public virtual bool IsSupported => PlatformSupport.Validate();

        public virtual bool IsInstalled => Component.IsInstalled;

        public abstract bool CanInstall { get; }

        protected TesseractLanguageData LanguageData { get; set; }

        public virtual IEnumerable<Language> InstalledLanguages => LanguageComponents.Where(x => x.IsInstalled).Select(x => LanguageData.LanguageMap[x.Id]);

        public virtual IEnumerable<Language> NotInstalledLanguages => LanguageComponents.Where(x => !x.IsInstalled).Select(x => LanguageData.LanguageMap[x.Id]);

        public virtual IExternalComponent Component => new ExternalComponent("ocr", Path.Combine(TesseractBasePath, TesseractExePath), DownloadInfo);

        public virtual IEnumerable<IExternalComponent> LanguageComponents => null;

        public virtual IEnumerable<OcrMode> SupportedModes => null;

        protected static readonly List<DownloadMirror> TesseractMirrors = new List<DownloadMirror>
        {
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://github.com/cyanfish/naps2-components/releases/download/tesseract-4.00b4/{0}"),
            new DownloadMirror(PlatformSupport.ModernWindows.Or(PlatformSupport.Linux), @"https://sourceforge.net/projects/naps2/files/components/tesseract-4.00b4/{0}/download"),
            new DownloadMirror(PlatformSupport.WindowsXp, @"http://xp-mirror.naps2.com/tesseract-4.00b4/{0}")
        };

        protected class RunInfo
        {
            public string Arguments { get; set; }

            public string PrefixPath { get; set; }

            public string DataPath { get; set; }
        }
    }
}
