using System.Collections.Immutable;
using System.Threading;
using System.Xml;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;

namespace NAPS2.Ocr;

public class TesseractOcrEngine : IOcrEngine
{
    private readonly string _tesseractPath;
    private readonly string? _languageDataBasePath;

    public TesseractOcrEngine(string tesseractPath, string? languageDataBasePath = null)
    {
        _tesseractPath = tesseractPath;
        _languageDataBasePath = languageDataBasePath;
    }
    
    public async Task<OcrResult?> ProcessImage(ScanningContext scanningContext, string imagePath, OcrParams ocrParams,
        CancellationToken cancelToken)
    {
        var logger = scanningContext.Logger;
        string tempHocrFilePath = Path.Combine(scanningContext.TempFolderPath, Path.GetRandomFileName());
        string tempHocrFilePathWithExt = tempHocrFilePath + ".hocr";
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _tesseractPath,
                Arguments = $"\"{imagePath}\" \"{tempHocrFilePath}\" -l {ocrParams.LanguageCode} hocr",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            if (_languageDataBasePath != null)
            {
                string subfolder = ocrParams.Mode == OcrMode.Best ? "best" : "fast";
                string languageDataPath = Path.Combine(_languageDataBasePath, subfolder);
                startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = languageDataPath;
                var tessdata = new DirectoryInfo(languageDataPath);
                EnsureHocrConfigExists(tessdata);
            }
            var tesseractProcess = Process.Start(startInfo);
            if (tesseractProcess == null)
            {
                // Couldn't start tesseract for some reason
                logger.LogError("Couldn't start OCR process.");
                return null;
            }
            // Improve main window responsiveness
            tesseractProcess.PriorityClass = ProcessPriorityClass.BelowNormal;

            var waitTasks = new List<Task>
            {
                tesseractProcess.WaitForExitAsync(),
                cancelToken.WaitHandle.WaitOneAsync()
            };
            var timeout = (int) (ocrParams.TimeoutInSeconds * 1000);
            if (timeout > 0)
            {
                waitTasks.Add(Task.Delay(timeout));
            }
            await Task.WhenAny(waitTasks);

            if (!tesseractProcess.HasExited)
            {
                if (!cancelToken.IsCancellationRequested)
                {
                    logger.LogError("OCR process timed out.");
                    OcrTimeout?.Invoke(this, EventArgs.Empty);
                }
                try
                {
                    tesseractProcess.Kill();
                    // Wait a bit to give the process time to release its file handles
                    Thread.Sleep(200);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error killing OCR process");
                }
                return null;
            }
#if DEBUG && DEBUGTESS
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
            var pageBounds = hocrDocument.Descendants()
                .Where(x => x.Attributes("class").Any(y => y.Value == "ocr_page"))
                .Select(x => GetBounds(x.Attribute("title")))
                .First();
            var elements = hocrDocument.Descendants()
                .Where(x => x.Attributes("class").Any(y => y.Value == "ocrx_word"))
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x =>
                {
                    var text = x.Value;
                    var lang = GetNearestAncestorAttribute(x, "lang") ?? "";
                    var rtl = GetNearestAncestorAttribute(x, "dir") == "rtl";
                    var bounds = GetBounds(x.Attribute("title"));
                    return new OcrResultElement(text, lang, rtl, bounds);
                }).ToImmutableList();
            return new OcrResult(pageBounds, elements);
        }
        catch (XmlException e)
        {
            logger.LogError(e, "Error running OCR");
            // Don't display to the error output as an xml exception may just indicate a normal OCR failure
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error running OCR");
            OcrError?.Invoke(this, new OcrErrorEventArgs(e));
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
                logger.LogError(e, "Error cleaning up OCR temp files");
            }
        }
    }

    public event EventHandler<OcrErrorEventArgs>? OcrError;

    public event EventHandler? OcrTimeout;

    private static string? GetNearestAncestorAttribute(XElement x, string attributeName)
    {
        var ancestor = x.AncestorsAndSelf().FirstOrDefault(x => x.Attribute(attributeName) != null);
        return ancestor?.Attribute(attributeName)?.Value;
    }

    private void EnsureHocrConfigExists(DirectoryInfo tessdata)
    {
        try
        {
            var configDir = new DirectoryInfo(Path.Combine(tessdata.FullName, "configs"));
            if (!configDir.Exists)
            {
                configDir.Create();
            }
            var hocrConfigFile = new FileInfo(Path.Combine(configDir.FullName, "hocr"));
            if (!hocrConfigFile.Exists)
            {
                using var writer = hocrConfigFile.CreateText();
                writer.Write("tessedit_create_hocr 1");
            }
        }
        catch (Exception)
        {
            // Possibly contention over creating the file. As long as it's created assume everything is okay.
            if (!File.Exists(Path.Combine(tessdata.FullName, "configs", "hocr")))
            {
                throw;
            }
        }
    }

    private (int x, int y, int w, int h) GetBounds(XAttribute? titleAttr)
    {
        var bounds = (0, 0, 0, 0);
        if (titleAttr != null)
        {
            foreach (var param in titleAttr.Value.Split(';'))
            {
                string[] parts = param.Trim().Split(' ');
                if (parts.Length == 5 && parts[0] == "bbox")
                {
                    int x1 = int.Parse(parts[1]), y1 = int.Parse(parts[2]);
                    int x2 = int.Parse(parts[3]), y2 = int.Parse(parts[4]);
                    bounds = (x1, y1, x2 - x1, y2 - y1);
                }
            }
        }
        return bounds;
    }
    
    // TODO: Consider adding back CanProcess, or otherwise using this code to get the languages from a system engine
//     private void CheckIfInstalled()
//     {
//         if (IsSupported && (_installCheckTime == null || _installCheckTime < DateTime.Now - TimeSpan.FromSeconds(2)))
//         {
//             try
//             {
//                 var process = Process.Start(new ProcessStartInfo
//                 {
//                     FileName = TesseractExePath,
//                     Arguments = "--list-langs",
//                     UseShellExecute = false,
//                     RedirectStandardOutput = true,
//                     RedirectStandardError = true
//                 });
//                 if (process != null && process.Id != 0)
//                 {
//                     var codes = process.StandardError.ReadToEnd().Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length == 3);
//                     _installedLanguages = codes.Select(code => LanguageData.LanguageMap.Get($"ocr-{code}")).WhereNotNull().ToList();
//                     _isInstalled = true;
//                     process.Kill();
//                 }
//             }
//             catch (Exception)
//             {
//                 // Component is not installed on the system path (or had an error)
//             }
//             _installCheckTime = DateTime.Now;
//         }
//     }
}