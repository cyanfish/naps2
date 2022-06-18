using System.Threading;

namespace NAPS2.Ocr;

public class TesseractOcrEngine : IOcrEngine
{
    private readonly string _exePath;
    private readonly string? _languageDataBasePath;

    public TesseractOcrEngine(string exePath, string? languageDataBasePath)
    {
        _exePath = exePath;
        _languageDataBasePath = languageDataBasePath;
    }
    
    public async Task<OcrResult?> ProcessImage(string imagePath, OcrParams ocrParams, CancellationToken cancelToken)
    {
        string tempHocrFilePath = Path.Combine(Paths.Temp, Path.GetRandomFileName());
        string tempHocrFilePathWithExt = tempHocrFilePath + ".hocr";
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _exePath,
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
                Log.Error("Couldn't start OCR process.");
                return null;
            }
            var timeout = (int) (ocrParams.TimeoutInSeconds * 1000);
            var stopwatch = Stopwatch.StartNew();
            // TODO: Need tests for this clas
            // TODO: Generalize
            var tcs = new TaskCompletionSource<object?>();
            tesseractProcess.Exited += (_, _) => tcs.SetResult(null);
            if (tesseractProcess.HasExited)
            {
                tcs.SetResult(null);
            }
            while (true)
            {
                // TODO: Clean up (i.e. use timeout/cancellation instead of interval checking)
                await Task.WhenAny(tcs.Task, Task.Delay(100));
                if (tesseractProcess.HasExited)
                {
                    break;
                }
                if (timeout != 0 && stopwatch.ElapsedMilliseconds >= timeout || cancelToken.IsCancellationRequested)
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
                .Select(x => new OcrResultElement(x.Value, GetBounds(x.Attribute("title"))));
            var rtl = false;
            // TODO: Can we detect rtl from the hocr file?
            // var rtl = _data.InstalledLanguages.Where(x => x.Code == ocrParams.LanguageCode).Select(x => x.RTL)
            //     .FirstOrDefault();
            return new OcrResult(pageBounds, elements, rtl);
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
            using var writer = hocrConfigFile.CreateText();
            writer.Write("tessedit_create_hocr 1");
        }
    }

    private (int x, int y, int w, int h) GetBounds(XAttribute titleAttr)
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
    
    
    // TODO: For local engine (where we need to manually direct to the language data)
    // public override TesseractRunInfo TesseractRunInfo(OcrParams ocrParams)
    // {
    //     OcrMode mode = ocrParams.Mode;
    //     string folder = mode == OcrMode.Fast || mode == OcrMode.Default ? "fast" : "best";
    //     if (ocrParams.LanguageCode.Split('+').All(code => !File.Exists(Path.Combine(TesseractBasePath, folder, $"{code.ToLowerInvariant()}.traineddata"))))
    //     {
    //         // Use the other source if the selected one doesn't exist
    //         folder = folder == "fast" ? "best" : "fast";
    //         mode = folder == "fast" ? OcrMode.Fast : OcrMode.Best;
    //     }
    //
    //     return new()
    //     {
    //         Arguments = mode == OcrMode.Best ? "--oem 1" : mode == OcrMode.Legacy ? "--oem 0" : "",
    //         DataPath = folder,
    //         PrefixPath = folder
    //     };
    
    // TODO: For system engine (where the language data is externally managed)
    // public override TesseractRunInfo TesseractRunInfo(OcrParams ocrParams) => new()
    // {
    //     Arguments = "",
    //     DataPath = null,
    //     PrefixPath = null
    // };
}