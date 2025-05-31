namespace NAPS2.Platform;

/// <summary>
/// Manages a user-level systemd service on Linux.
/// </summary>
public class LinuxServiceManager : IOsServiceManager
{
    // At the moment we only support systemd on Linux and without Flatpak
    public bool CanRegister => Directory.Exists("/run/systemd/system/") && !File.Exists("/.flatpak-info");

    private static string UnitPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config/systemd/user/naps2-sharing-server.service");
    
    public bool IsRegistered => File.Exists(UnitPath);

    public void Register()
    {
        var unitDef = $"""
                          [Unit]
                          Description=NAPS2 Scanner Sharing Server
                          
                          [Service]
                          Type=simple
                          Restart=always
                          RestartSec=1
                          ExecStart={Environment.ProcessPath} server
                          
                          [Install]
                          WantedBy=default.target
                          """;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UnitPath)!);
            File.WriteAllText(UnitPath, unitDef);
        }
        catch (Exception ex)
        {
            Log.ErrorException($"Error creating systemd unit: {UnitPath}", ex);
        }
        if (!ProcessHelper.TryRun("systemctl", "--user daemon-reload", 1000))
        {
            Log.Error("Could not run systemctl daemon-reload");
        }
        if (!ProcessHelper.TryRun("systemctl", "--user enable naps2-sharing-server", 1000))
        {
            Log.Error("Could not enable service naps2-sharing-server");
        }
        if (!ProcessHelper.TryRun("systemctl", "--user start naps2-sharing-server", 1000))
        {
            Log.Error("Could not start service naps2-sharing-server");
        }
    }

    public void Unregister()
    {
        if (!ProcessHelper.TryRun("systemctl", "--user stop naps2-sharing-server", 1000))
        {
            Log.Error("Could not stop service naps2-sharing-server");
        }
        if (!ProcessHelper.TryRun("systemctl", "--user disable naps2-sharing-server", 1000))
        {
            Log.Error("Could not disable service naps2-sharing-server");
        }
        try
        {
            File.Delete(UnitPath);
        }
        catch (Exception ex)
        {
            Log.ErrorException($"Error deleting systemd unit: {UnitPath}", ex);
        }
        if (!ProcessHelper.TryRun("systemctl", "--user daemon-reload", 1000))
        {
            Log.Error("Could not run systemctl daemon-reload");
        }
    }
}