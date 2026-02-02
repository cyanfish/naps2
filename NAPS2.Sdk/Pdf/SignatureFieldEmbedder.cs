using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NAPS2.Pdf;

/// <summary>
/// Helper class to embed signature fields into PDFs using pyHanko (Python).
/// </summary>
public class SignatureFieldEmbedder
{
    private readonly ILogger _logger;

    public SignatureFieldEmbedder(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Embeds signature fields into a PDF file using pyHanko.
    /// </summary>
    /// <param name="inputPdfPath">Path to the input PDF file</param>
    /// <param name="outputPdfPath">Path to the output PDF file</param>
    /// <param name="fields">List of signature field placements with page dimensions</param>
    /// <param name="pageDimensions">Dictionary mapping page index to page dimensions (width, height) in PDF points</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool EmbedFields(string inputPdfPath, string outputPdfPath,
        List<(int pageIndex, SignatureFieldPlacement field, double pageWidth, double pageHeight, double imageWidth, double imageHeight)> fields,
        Dictionary<int, (double width, double height)> pageDimensions)
    {
        if (fields.Count == 0)
        {
            // No fields to embed, just copy the file
            File.Copy(inputPdfPath, outputPdfPath, true);
            return true;
        }

        // Find Python executable
        Console.WriteLine("=== Starting signature field embedding ===");
        Console.WriteLine($"Input PDF: {inputPdfPath}");
        Console.WriteLine($"Output PDF: {outputPdfPath}");
        Console.WriteLine($"Number of fields to embed: {fields.Count}");
        
        _logger.LogInformation("=== Starting signature field embedding ===");
        _logger.LogInformation("Input PDF: {InputPath}", inputPdfPath);
        _logger.LogInformation("Output PDF: {OutputPath}", outputPdfPath);
        _logger.LogInformation("Number of fields to embed: {FieldCount}", fields.Count);

        // Prefer bundled helper executable (no Python interpreter needed)
        var helperExe = FindBundledHelper();
        if (helperExe != null)
        {
            Console.WriteLine($"Using bundled helper: {helperExe}");
            _logger.LogInformation("Using bundled signature helper: {HelperExe}", helperExe);

            // Convert fields to JSON format expected by the helper
            var helperFieldsJson = ConvertFieldsToJson(fields, pageDimensions);
            Console.WriteLine($"Fields JSON: {helperFieldsJson}");
            _logger.LogInformation("Fields JSON: {FieldsJson}", helperFieldsJson);

            try
            {
                var arguments = $"\"{inputPdfPath}\" \"{outputPdfPath}\" \"{helperFieldsJson.Replace("\"", "\\\"")}\"";
                Console.WriteLine($"Executing: {helperExe} {arguments}");
                _logger.LogInformation("Executing: {HelperExe} {Arguments}", helperExe, arguments);

                var startInfo = new ProcessStartInfo
                {
                    FileName = helperExe,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    _logger.LogError("Failed to start signature helper process");
                    File.Copy(inputPdfPath, outputPdfPath, true);
                    return false;
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine($"Signature helper process exited with code: {process.ExitCode}");
                _logger.LogInformation("Signature helper process exited with code: {ExitCode}", process.ExitCode);
                if (!string.IsNullOrEmpty(output))
                {
                    Console.WriteLine($"Helper stdout: {output}");
                    _logger.LogInformation("Helper stdout: {Output}", output);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Helper stderr: {error}");
                    _logger.LogWarning("Helper stderr: {Error}", error);
                }

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Signature fields embedded successfully!");
                    _logger.LogInformation("Signature fields embedded successfully");
                    return true;
                }
                else if (process.ExitCode == 2)
                {
                    Console.WriteLine("ERROR: pyHanko not installed. Install with: pip install pyHanko");
                    _logger.LogWarning("pyHanko not installed. Signature fields will not be embedded. Install with: pip install pyHanko");
                    File.Copy(inputPdfPath, outputPdfPath, true);
                    return false;
                }
                else
                {
                    Console.WriteLine($"ERROR: Failed to embed signature fields. Exit code: {process.ExitCode}");
                    _logger.LogError("Failed to embed signature fields. Exit code: {ExitCode}", process.ExitCode);
                    File.Copy(inputPdfPath, outputPdfPath, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while embedding signature fields");
                File.Copy(inputPdfPath, outputPdfPath, true);
                return false;
            }
        }

        Console.WriteLine("Bundled helper not found, falling back to Python script");
        _logger.LogInformation("Bundled signature helper not found, falling back to Python script");

        var pythonExe = FindPythonExecutable();
        if (pythonExe == null)
        {
            Console.WriteLine("ERROR: Python executable not found");
            _logger.LogWarning("Python executable not found. Signature fields will not be embedded.");
            File.Copy(inputPdfPath, outputPdfPath, true);
            return false;
        }
        Console.WriteLine($"Using Python: {pythonExe}");
        _logger.LogInformation("Using Python: {PythonExe}", pythonExe);

        // Find the script path
        var scriptPath = FindScriptPath();
        if (scriptPath == null || !File.Exists(scriptPath))
        {
            Console.WriteLine($"ERROR: Script not found at: {scriptPath}");
            _logger.LogWarning("Signature field embedding script not found at: {ScriptPath}", scriptPath);
            File.Copy(inputPdfPath, outputPdfPath, true);
            return false;
        }
        Console.WriteLine($"Using script: {scriptPath}");
        _logger.LogInformation("Using script: {ScriptPath}", scriptPath);

        // Convert fields to JSON format expected by Python script
        var fieldsJson = ConvertFieldsToJson(fields, pageDimensions);
        Console.WriteLine($"Fields JSON: {fieldsJson}");
        _logger.LogInformation("Fields JSON: {FieldsJson}", fieldsJson);

        try
        {
            // Invoke Python script
            var arguments = $"\"{scriptPath}\" \"{inputPdfPath}\" \"{outputPdfPath}\" \"{fieldsJson.Replace("\"", "\\\"")}\"";
            Console.WriteLine($"Executing: {pythonExe} {arguments}");
            _logger.LogInformation("Executing: {PythonExe} {Arguments}", pythonExe, arguments);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError("Failed to start Python process");
                File.Copy(inputPdfPath, outputPdfPath, true);
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine($"Python process exited with code: {process.ExitCode}");
            _logger.LogInformation("Python process exited with code: {ExitCode}", process.ExitCode);
            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine($"Python stdout: {output}");
                _logger.LogInformation("Python stdout: {Output}", output);
            }
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Python stderr: {error}");
                _logger.LogWarning("Python stderr: {Error}", error);
            }

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Signature fields embedded successfully!");
                _logger.LogInformation("Signature fields embedded successfully");
                return true;
            }
            else if (process.ExitCode == 2)
            {
                Console.WriteLine("ERROR: pyHanko not installed. Install with: pip install pyHanko");
                _logger.LogWarning("pyHanko not installed. Signature fields will not be embedded. Install with: pip install pyHanko");
                File.Copy(inputPdfPath, outputPdfPath, true);
                return false;
            }
            else
            {
                Console.WriteLine($"ERROR: Failed to embed signature fields. Exit code: {process.ExitCode}");
                _logger.LogError("Failed to embed signature fields. Exit code: {ExitCode}", process.ExitCode);
                File.Copy(inputPdfPath, outputPdfPath, true);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while embedding signature fields");
            File.Copy(inputPdfPath, outputPdfPath, true);
            return false;
        }
    }

    /// <summary>
    /// Attempts to find the bundled signature helper executable shipped alongside the application.
    /// </summary>
    /// <remarks>
    /// This is preferred over the Python script path because it does not require a Python interpreter.
    /// Search is performed relative to <see cref="AppDomain.CurrentDomain" />.<see cref="AppDomain.BaseDirectory" />.
    /// </remarks>
    /// <returns>The full path to the helper executable if found; otherwise, <c>null</c>.</returns>
    private string? FindBundledHelper()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        Console.WriteLine($"App directory: {appDir}");

        var helperBaseName = "naps2-signature-helper";
        var helperNames = new[] { helperBaseName + ".exe", helperBaseName };

        var possibleDirs = new List<string>
        {
            appDir,
            Path.Combine(appDir, "tools"),
            Path.Combine(appDir, "..", "tools"),
            Path.Combine(appDir, "..", "..", "tools"),
        };

        // Additional macOS bundle-specific checks
        // If running from within a .app bundle, locate the Contents directory.
        var dirInfo = new DirectoryInfo(Path.GetFullPath(appDir));
        DirectoryInfo? contentsDir = null;
        while (dirInfo != null)
        {
            if (string.Equals(dirInfo.Name, "Contents", StringComparison.OrdinalIgnoreCase) &&
                dirInfo.Parent != null &&
                dirInfo.Parent.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
            {
                contentsDir = dirInfo;
                break;
            }

            dirInfo = dirInfo.Parent;
        }

        if (contentsDir != null)
        {
            possibleDirs.Add(Path.Combine(contentsDir.FullName, "MacOS"));
            possibleDirs.Add(Path.Combine(contentsDir.FullName, "tools"));
        }

        foreach (var dir in possibleDirs)
        {
            foreach (var helperName in helperNames)
            {
                var fullPath = Path.GetFullPath(Path.Combine(dir, helperName));
                Console.WriteLine($"Checking bundled helper: {fullPath}");
                if (File.Exists(fullPath))
                {
                    Console.WriteLine($"Found bundled helper at: {fullPath}");
                    _logger.LogInformation("Found bundled signature helper at: {HelperPath}", fullPath);
                    return fullPath;
                }
            }
        }

        Console.WriteLine("Bundled signature helper not found in any of the checked paths");
        _logger.LogInformation("Bundled signature helper not found in any of the checked paths");
        return null;
    }

    private string? FindPythonExecutable()
    {
        // First, try to find Python in a virtual environment relative to the script
        var scriptPath = FindScriptPath();
        if (scriptPath != null)
        {
            var scriptDir = Path.GetDirectoryName(scriptPath);
            if (scriptDir != null)
            {
                var repoRoot = Path.GetFullPath(Path.Combine(scriptDir, ".."));
                var venvPython = Path.Combine(repoRoot, ".venv", "bin", "python");
                if (File.Exists(venvPython))
                {
                    _logger.LogInformation("Found Python in virtual environment: {VenvPython}", venvPython);
                    return venvPython;
                }
            }
        }
        
        // Try common Python executable names in PATH
        var pythonNames = new[] { "python3", "python", "py" };
        
        foreach (var name in pythonNames)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = name,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        return name;
                    }
                }
            }
            catch
            {
                // Continue to next name
            }
        }

        return null;
    }

    private string? FindScriptPath()
    {
        // Try to find the script relative to the application directory
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        Console.WriteLine($"App directory: {appDir}");
        
        var possiblePaths = new[]
        {
            Path.Combine(appDir, "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "..", "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "..", "..", "..", "..", "scripts", "embed_signature_fields.py"),
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            Console.WriteLine($"Checking: {fullPath}");
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"Found script at: {fullPath}");
                return fullPath;
            }
        }

        Console.WriteLine("Script not found in any of the checked paths");
        return null;
    }

    private string ConvertFieldsToJson(
        List<(int pageIndex, SignatureFieldPlacement field, double pageWidth, double pageHeight, double imageWidth, double imageHeight)> fields,
        Dictionary<int, (double width, double height)> pageDimensions)
    {
        var jsonFields = fields.Select(f =>
        {
            // Convert normalized coordinates to PDF points
            // The normalized coordinates were created using the original image dimensions (in pixels),
            // but we need to convert them to PDF page dimensions (in points).
            // PDF uses bottom-left origin, so we need to flip Y coordinate.
            
            var pageWidth = f.pageWidth;
            var pageHeight = f.pageHeight;
            var imageWidth = f.imageWidth;
            var imageHeight = f.imageHeight;
            
            Console.WriteLine($"Converting field {f.field.FieldName}:");
            Console.WriteLine($"  Normalized: X={f.field.NormalizedX}, Y={f.field.NormalizedY}, W={f.field.NormalizedWidth}, H={f.field.NormalizedHeight}");
            Console.WriteLine($"  Image dimensions (pixels): W={imageWidth}, H={imageHeight}");
            Console.WriteLine($"  Page dimensions (PDF points): W={pageWidth}, H={pageHeight}");
            
            // First convert from normalized to image pixel coordinates
            var (imagePixelX, imagePixelY, imagePixelWidth, imagePixelHeight) = f.field.ToPixels(
                (float)imageWidth,
                (float)imageHeight);

            Console.WriteLine($"  Image pixel coords: X={imagePixelX}, Y={imagePixelY}, W={imagePixelWidth}, H={imagePixelHeight}");
            
            // Then scale from image pixels to PDF points
            var scaleX = pageWidth / imageWidth;
            var scaleY = pageHeight / imageHeight;
            
            var pdfX = imagePixelX * scaleX;
            var pdfWidth = imagePixelWidth * scaleX;
            var pdfHeight = imagePixelHeight * scaleY;
            
            // Convert Y coordinate from top-left origin (UI) to bottom-left origin (PDF).
            //
            // UI coordinates:
            //   imagePixelY = distance from TOP of the image.
            // PDF coordinates:
            //   y = distance from BOTTOM of the page.
            //
            // We want the lower-left corner of the widget rectangle in PDF user space:
            //   bottomOfFieldFromTop = imagePixelY + imagePixelHeight
            //   bottomOfFieldFromBottom = imageHeight - bottomOfFieldFromTop
            //   pdfY = bottomOfFieldFromBottom * scaleY
            var pdfY = (imageHeight - imagePixelY - imagePixelHeight) * scaleY;
            
            Console.WriteLine($"  Scale factors: X={scaleX}, Y={scaleY}");
            Console.WriteLine($"  PDF coordinates: X={pdfX}, Y={pdfY}, W={pdfWidth}, H={pdfHeight}");

            return new
            {
                name = f.field.FieldName,
                page = f.pageIndex,
                x = pdfX,
                y = pdfY,
                width = pdfWidth,
                height = pdfHeight
            };
        }).ToList();

        return JsonConvert.SerializeObject(jsonFields);
    }
}
