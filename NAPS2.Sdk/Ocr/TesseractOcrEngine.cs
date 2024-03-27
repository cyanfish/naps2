using System.Collections.Immutable;
using System.Threading;
using System.Xml;
using Microsoft.Extensions.Logging;
using NAPS2.Scan;
using NAPS2.Unmanaged;
using Bounds = (int x, int y, int w, int h);

namespace NAPS2.Ocr;

/// <summary>
/// OCR engine using Tesseract (https://github.com/tesseract-ocr/tesseract).
/// </summary>
public class TesseractOcrEngine : IOcrEngine
{
    private readonly string _tesseractPath;
    private readonly string? _languageDataBasePath;
    private readonly bool _withModes;

    /// <summary>
    /// Gets a TesseractOcrEngine instance configured to use the Tesseract executable on the system PATH with the
    /// system-installed language data.
    /// </summary>
    public static TesseractOcrEngine System() =>
        new("tesseract");

    /// <summary>
    /// Gets a TesseractOcrEngine instance configured to use the Tesseract executable from the NAPS2.Tesseract.Binaries
    /// nuget package using language data .traineddata files in the specified folder.
    /// </summary>
    public static TesseractOcrEngine Bundled(string languageDataPath) =>
        new(BundlePath, languageDataPath, false);

    /// <summary>
    /// Gets a TesseractOcrEngine instance configured to use the Tesseract executable from the NAPS2.Tesseract.Binaries
    /// nuget package using language data .traineddata files in the specified folder. The folder is expected to have
    /// subfolders named "best" and "fast" with the actual .trainneddata files that will be used based on the OcrMode.
    /// </summary>
    public static TesseractOcrEngine BundledWithModes(string languageDataBasePath) =>
        new(BundlePath, languageDataBasePath, true);

    /// <summary>
    /// Gets a TesseractOcrEngine instance configured to use the specified Tesseract executable, optionally looking for
    /// .traineddata files in the specified folder.
    /// </summary>
    public static TesseractOcrEngine Custom(string tesseractExePath, string? languageDataPath = null) =>
        new(tesseractExePath, languageDataPath, false);

    /// <summary>
    /// Gets a TesseractOcrEngine instance configured to use the specified Tesseract executable using language data
    /// .traineddata files in the specified folder. The folder is expected to have subfolders named "best" and "fast"
    /// with the actual .trainneddata files that will be used based on the OcrMode.
    /// </summary>
    public static TesseractOcrEngine CustomWithModes(string tesseractExePath, string languageDataBasePath) =>
        new(tesseractExePath, languageDataBasePath, true);

    private static string BundlePath => NativeLibrary.FindExePath(PlatformCompat.System.TesseractExecutableName);

    private TesseractOcrEngine(string tesseractPath, string? languageDataBasePath = null, bool withModes = true)
    {
        _tesseractPath = tesseractPath;
        _languageDataBasePath = languageDataBasePath;
        _withModes = withModes;
    }

    public async Task<OcrResult?> ProcessImage(ScanningContext scanningContext, string imagePath, OcrParams ocrParams,
        CancellationToken cancelToken)
    {
        var logger = scanningContext.Logger;
        string tempHocrFilePath = Path.Combine(scanningContext.TempFolderPath, Path.GetRandomFileName());
        string tempHocrFilePathWithExt = tempHocrFilePath + ".hocr";
        try
        {
            if (ocrParams.Mode.HasFlag(OcrMode.WithPreProcess))
            {
                PreProcessImage(scanningContext, imagePath);
            }
            var configVals = "-c tessedit_create_hocr=1 -c hocr_font_info=1";
            var startInfo = new ProcessStartInfo
            {
                FileName = _tesseractPath,
                Arguments = $"\"{imagePath}\" \"{tempHocrFilePath}\" -l {ocrParams.LanguageCode} {configVals}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            if (_languageDataBasePath != null)
            {
                string languageDataPath = _languageDataBasePath;
                if (_withModes)
                {
                    string subfolder = ocrParams.Mode.HasFlag(OcrMode.Best) ? "best" : "fast";
                    languageDataPath = Path.Combine(languageDataPath, subfolder);
                }
                startInfo.EnvironmentVariables["TESSDATA_PREFIX"] = languageDataPath;
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
            return CreateOcrResult(hocrDocument);
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

    private static void PreProcessImage(ScanningContext scanningContext, string imagePath)
    {
        IMemoryImage? image = null;
        try
        {
            image = scanningContext.ImageContext.Load(imagePath);
            image = image.PerformTransform(new CorrectionTransform(CorrectionMode.Document));
            image.Save(imagePath);
        }
        finally
        {
            image?.Dispose();
        }
    }

    private OcrResult CreateOcrResult(XDocument hocrDocument)
    {
        var pageBounds = hocrDocument.Descendants()
            .Where(element => GetClass(element) == "ocr_page")
            .Select(GetBounds)
            .First();
        var words = new List<OcrResultElement>();
        var lines = new List<OcrResultElement>();
        foreach (var lineElement in hocrDocument.Descendants()
                     .Where(element => GetClass(element) is "ocr_line" or "ocr_header" or "ocr_textfloat"))
        {
            var lineBounds = GetBounds(lineElement);
            var lineAngle = GetTextAngle(lineElement);
            bool isRotated = lineAngle is >= 45 or <= -45;
            var baselineParams = GetBaselineParams(lineElement);
            var lineWords = lineElement.Descendants()
                .Where(element => GetClass(element) == "ocrx_word")
                .Where(element => !string.IsNullOrWhiteSpace(element.Value))
                .Select(wordElement =>
                {
                    var wordBounds = GetBounds(wordElement);
                    return new OcrResultElement(
                        wordElement.Value,
                        GetNearestAncestorAttribute(wordElement, "lang") ?? "",
                        GetNearestAncestorAttribute(wordElement, "dir") == "rtl",
                        wordBounds,
                        // TODO: Maybe we can properly handle rotated text?
                        isRotated
                            ? wordBounds.y + wordBounds.h
                            : CalculateBaseline(baselineParams, lineBounds, wordBounds),
                        GetFontSize(wordElement),
                        ImmutableList<OcrResultElement>.Empty);
                }).ToImmutableList();
            if (lineWords.Count == 0) continue;
            words.AddRange(lineWords);
            lines.Add(lineWords[0] with
            {
                Text = string.Join(" ", lineWords.Select(x => x.Text)),
                Bounds = lineBounds,
                Baseline = CalculateBaseline(baselineParams, lineBounds, lineBounds),
                Children = lineWords
            });
        }
        return new OcrResult(pageBounds, words.ToImmutableList(), lines.ToImmutableList());
    }

    private static string? GetNearestAncestorAttribute(XElement x, string attributeName)
    {
        var ancestor = x.AncestorsAndSelf().FirstOrDefault(x => x.Attribute(attributeName) != null);
        return ancestor?.Attribute(attributeName)?.Value;
    }

    private string? GetClass(XElement? element)
    {
        return element?.Attribute("class")?.Value;
    }

    private bool ParseData(XElement? element, string dataKey, int dataCount, out string[] parts)
    {
        parts = Array.Empty<string>();
        var titleAttr = element?.Attribute("title");
        if (titleAttr != null)
        {
            foreach (var param in titleAttr.Value.Split(';'))
            {
                parts = param.Trim().Split(' ');
                if (parts[0] == dataKey && parts.Length == dataCount + 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Bounds GetBounds(XElement? element)
    {
        var bounds = (0, 0, 0, 0);
        if (ParseData(element, "bbox", 4, out string[] parts))
        {
            int x1 = int.Parse(parts[1]), y1 = int.Parse(parts[2]);
            int x2 = int.Parse(parts[3]), y2 = int.Parse(parts[4]);
            bounds = (x1, y1, x2 - x1, y2 - y1);
        }
        return bounds;
    }

    private int GetFontSize(XElement? element)
    {
        int fontSize = 0;
        if (ParseData(element, "x_fsize", 1, out string[] parts))
        {
            fontSize = int.Parse(parts[1]);
        }
        return fontSize;
    }

    private (float m, float b) GetBaselineParams(XElement? element)
    {
        float m = 0;
        float b = 0;
        if (ParseData(element, "baseline", 2, out string[] parts))
        {
            m = float.Parse(parts[1]);
            b = float.Parse(parts[2]);
        }
        return (m, b);
    }

    private float GetTextAngle(XElement? element)
    {
        float angle = 0;
        if (ParseData(element, "textangle", 1, out string[] parts))
        {
            angle = float.Parse(parts[1]);
        }
        return angle;
    }

    private int CalculateBaseline((float m, float b) baselineParams, Bounds lineBounds, Bounds elementBounds)
    {
        // The line baseline is a linear equation (y=mx + b), so we calculate the word baseline from the
        // word offset to the left side of the line.
        float midpoint = elementBounds.x + elementBounds.w / 2f;
        int relativeBaseline = (int) Math.Round(baselineParams.b +
                                                baselineParams.m * (midpoint - lineBounds.x));
        int absoluteBaseline = relativeBaseline + lineBounds.y + lineBounds.h;
        return absoluteBaseline;
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