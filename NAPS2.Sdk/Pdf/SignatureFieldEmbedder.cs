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
    /// <param name="pageHeights">Dictionary mapping page index to page height in PDF points</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool EmbedFields(string inputPdfPath, string outputPdfPath, 
        List<(int pageIndex, SignatureFieldPlacement field, double pageHeight)> fields,
        Dictionary<int, double> pageHeights)
    {
        if (fields.Count == 0)
        {
            // No fields to embed, just copy the file
            File.Copy(inputPdfPath, outputPdfPath, true);
            return true;
        }

        // Find Python executable
        var pythonExe = FindPythonExecutable();
        if (pythonExe == null)
        {
            _logger.LogWarning("Python executable not found. Signature fields will not be embedded.");
            File.Copy(inputPdfPath, outputPdfPath, true);
            return false;
        }

        // Find the script path
        var scriptPath = FindScriptPath();
        if (scriptPath == null || !File.Exists(scriptPath))
        {
            _logger.LogWarning("Signature field embedding script not found at: {ScriptPath}", scriptPath);
            File.Copy(inputPdfPath, outputPdfPath, true);
            return false;
        }

        // Convert fields to JSON format expected by Python script
        var fieldsJson = ConvertFieldsToJson(fields, pageHeights);

        try
        {
            // Invoke Python script
            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" \"{inputPdfPath}\" \"{outputPdfPath}\" \"{fieldsJson.Replace("\"", "\\\"")}\"",
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

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Signature fields embedded successfully: {Output}", output);
                return true;
            }
            else if (process.ExitCode == 2)
            {
                _logger.LogWarning("pyHanko not installed. Signature fields will not be embedded. Install with: pip install pyHanko");
                File.Copy(inputPdfPath, outputPdfPath, true);
                return false;
            }
            else
            {
                _logger.LogError("Failed to embed signature fields. Exit code: {ExitCode}, Error: {Error}", 
                    process.ExitCode, error);
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

    private string? FindPythonExecutable()
    {
        // Try common Python executable names
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
        var possiblePaths = new[]
        {
            Path.Combine(appDir, "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "scripts", "embed_signature_fields.py"),
            Path.Combine(appDir, "..", "..", "..", "..", "scripts", "embed_signature_fields.py"),
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private string ConvertFieldsToJson(
        List<(int pageIndex, SignatureFieldPlacement field, double pageHeight)> fields,
        Dictionary<int, double> pageHeights)
    {
        var jsonFields = fields.Select(f =>
        {
            // Convert normalized coordinates to PDF points
            // PDF uses bottom-left origin, so we need to flip Y coordinate
            var pageHeight = f.pageHeight;
            var (pixelX, pixelY, pixelWidth, pixelHeight) = f.field.ToPixels(
                (float)pageHeight, // Using page height as a proxy for width (will be scaled correctly)
                (float)pageHeight);

            // In PDF coordinates (bottom-left origin)
            var pdfY = pageHeight - pixelY - pixelHeight;

            return new
            {
                name = f.field.FieldName,
                page = f.pageIndex,
                x = pixelX,
                y = pdfY,
                width = pixelWidth,
                height = pixelHeight
            };
        }).ToList();

        return JsonConvert.SerializeObject(jsonFields);
    }
}
