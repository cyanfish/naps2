using Microsoft.Win32;
using MimeKit;
using NAPS2.Scan;

namespace NAPS2.ImportExport.Email;

/// <summary>
/// The "New" Outlook. https://support.microsoft.com/en-us/office/getting-started-with-the-new-outlook-for-windows-656bb8d9-5a60-49b2-a98b-ba7822bc7627
/// </summary>
internal class OutlookNewEmailProvider : MimeEmailProvider
{
    private readonly ScanningContext _scanningContext;
    private readonly ErrorOutput _errorOutput;

    public OutlookNewEmailProvider(ScanningContext scanningContext, ErrorOutput errorOutput)
    {
        _scanningContext = scanningContext;
        _errorOutput = errorOutput;
    }

    private string? ExecutablePath
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return null;
            using var key =
                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\olk.exe");
            var path = key?.GetValue(null)?.ToString();
            if (path == null || !File.Exists(path)) return null;
            return path;
        }
    }

    public bool IsAvailable => ExecutablePath != null;

    protected override async Task SendMimeMessage(MimeMessage message, ProgressHandler progress, bool autoSend)
    {
        await Task.Run(async () =>
        {
            if (progress.CancelToken.IsCancellationRequested) return;

            string? emlPath = null;
            try
            {
                var exePath = ExecutablePath ??
                              throw new InvalidOperationException("New outlook executable not available");

                // Add header so that Outlook opens the message as a draft to edit/send
                message.Headers.Add("X-Unsent", "1");

                emlPath = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName() + ".eml");
                await message.WriteToAsync(emlPath, progress.CancelToken);

                var process =
                    Process.Start(new ProcessStartInfo(exePath, emlPath)
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                if (process == null)
                {
                    throw new InvalidOperationException("Could not start Outlook (new) process");
                }
                await Task.WhenAny(
                    process.WaitForExitAsync(),
                    Task.Delay(TimeSpan.FromSeconds(30)),
                    progress.CancelToken.WaitHandle.WaitOneAsync());
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _errorOutput.DisplayError(MiscResources.EmailError, ex);
            }
            finally
            {
                if (emlPath != null)
                {
                    try
                    {
                        File.Delete(emlPath);
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }
        });
    }
}