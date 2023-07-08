using System.Text;

namespace NAPS2.ImportExport.Email;

public class ThunderbirdEmailProvider : IEmailProvider
{
    private readonly ErrorOutput _errorOutput;

    public ThunderbirdEmailProvider(ErrorOutput errorOutput)
    {
        _errorOutput = errorOutput;
    }

    // Note we can't really support the Flatpak version of Thunderbird as it won't have access to attachment files from
    // the sandbox.
    public bool IsAvailable => ProcessHelper.IsSuccessful("thunderbird", "-v", 1000);

    public Task<bool> SendEmail(EmailMessage message, ProgressHandler progress = default)
    {
        return Task.Run(async () =>
        {
            var arguments = new List<string>();
            string? bodyFile = null;
            try
            {
                if (message.Attachments.Any())
                {
                    // TODO: Need to use name if different than path (i.e. copy to temp)
                    arguments.Add($"attachment='{string.Join(",", message.Attachments.Select(x => x.FilePath))}'");
                }
                if (!string.IsNullOrEmpty(message.BodyText))
                {
                    bodyFile = Path.Combine(Paths.Temp, Path.GetRandomFileName() + ".txt");
                    File.WriteAllText(bodyFile, message.BodyText, Encoding.UTF8);
                    arguments.Add($"message='{bodyFile}'");
                }
                if (!string.IsNullOrEmpty(message.Subject))
                {
                    // There doesn't seem to be a way to escape "'," but it shouldn't be common
                    arguments.Add($"subject='{message.Subject!.Replace("',", "' ,")}'");
                }
                if (message.Recipients.Any(x => x.Type == EmailRecipientType.To))
                {
                    arguments.Add(
                        $"to='{string.Join(",", message.Recipients.Where(x => x.Type == EmailRecipientType.To).Select(x => x.Address))}'");
                }
                if (message.Recipients.Any(x => x.Type == EmailRecipientType.Cc))
                {
                    arguments.Add(
                        $"cc='{string.Join(",", message.Recipients.Where(x => x.Type == EmailRecipientType.Cc).Select(x => x.Address))}'");
                }
                if (message.Recipients.Any(x => x.Type == EmailRecipientType.Bcc))
                {
                    arguments.Add(
                        $"bcc='{string.Join(",", message.Recipients.Where(x => x.Type == EmailRecipientType.Bcc).Select(x => x.Address))}'");
                }
                // This escaping isn't perfect but it should be good enough. I can't identify clear rules used by the
                // thunderbird parser anyway.
                var composeArgs = string.Join(",", arguments.Select(x => x.Replace("\"", "\\\"")));
                var process =
                    Process.Start(new ProcessStartInfo("thunderbird", $"-compose \"{composeArgs}\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                if (process == null)
                {
                    throw new InvalidOperationException("Could not start Thunderbird process");
                }
                await process.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                _errorOutput.DisplayError(MiscResources.EmailError, ex);
                return false;
            }
            finally
            {
                if (bodyFile != null)
                {
                    try
                    {
                        File.Delete(bodyFile);
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }
            return true;
        });
    }
}