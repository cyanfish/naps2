namespace NAPS2.Platform;

/// <summary>
/// Manages a user-level launchd (https://www.launchd.info/) service on macOS.
/// </summary>
public class MacServiceManager : IOsServiceManager
{
    private const string SERVICE_NAME = "com.naps2.ScannerSharing";

    private static string PlistPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $"Library/LaunchAgents/{SERVICE_NAME}.plist");

    public bool CanRegister => true;

    public bool IsRegistered => File.Exists(PlistPath);

    public bool Register()
    {
        var serviceDef = $"""
                          <?xml version="1.0" encoding="UTF-8"?>
                          <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                          <plist version="1.0">
                          	<dict>
                          		<key>Label</key>
                          		<string>{SERVICE_NAME}</string>
                          		<key>Program</key>
                          		<string>{Environment.ProcessPath}</string>
                          		<key>ProgramArguments</key>
                          		<array>
                          			<string>{Environment.ProcessPath}</string>
                          			<string>server</string>
                          		</array>
                          		<key>RunAtLoad</key>
                          		<true/>
                          	</dict>
                          </plist>
                          """;
        try
        {
            File.WriteAllText(PlistPath, serviceDef);
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error creating sharing service PLIST", ex);
        }
        if (!ProcessHelper.TryRun("launchctl", $"load \"{PlistPath}\"", 1000))
        {
            Log.Error($"Could not load service {SERVICE_NAME}");
            return false;
        }
        return true;
    }

    public void Unregister()
    {
        // TODO: Longer timeout / run async?
        if (!ProcessHelper.TryRun("launchctl", $"unload \"{PlistPath}\"", 1000))
        {
            Log.Error($"Could not unload service {SERVICE_NAME}");
        }
        try
        {
            File.Delete(PlistPath);
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error deleting sharing service PLIST", ex);
        }
    }
}