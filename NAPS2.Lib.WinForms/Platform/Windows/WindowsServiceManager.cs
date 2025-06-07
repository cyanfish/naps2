using System.Runtime.InteropServices.ComTypes;
using NAPS2.Remoting;

namespace NAPS2.Platform.Windows;

/// <summary>
/// Manages a user startup item on Windows.
/// </summary>
public class WindowsServiceManager(ProcessCoordinator processCoordinator)
    : IOsServiceManager
{
    private static string ShortcutPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft\\Windows\\Start Menu\\Programs\\Startup\\NAPS2 Scanner Sharing.lnk");

    public bool CanRegister => true;

    public bool IsRegistered => File.Exists(ShortcutPath);

    public bool Register()
    {
        try
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            var shortcut = (Win32.IShellLink) new Win32.ShellLink();
            shortcut.SetWorkingDirectory(AssemblyHelper.EntryFolder);
            shortcut.SetPath(Environment.ProcessPath!);
            shortcut.SetArguments("server");
            shortcut.SetIconLocation(Environment.ProcessPath!, 0);
            var persistFile = (IPersistFile) shortcut;
            persistFile.Save(ShortcutPath, false);
        }
        catch (Exception ex)
        {
            Log.ErrorException($"Error saving startup shortcut to {ShortcutPath}", ex);
        }
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                Arguments = "server"
            });
            return process != null;
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error starting NAPS2 server process", ex);
            return false;
        }
    }

    public void Unregister()
    {
        try
        {
            File.Delete(ShortcutPath);
        }
        catch (Exception ex)
        {
            Log.ErrorException($"Error deleting startup shortcut at {ShortcutPath}", ex);
        }
        try
        {
            int stopped = 0;
            foreach (var process in WindowsApplicationLifecycle.GetOtherNaps2Processes())
            {
                if (processCoordinator.StopSharingServer(process, 100))
                {
                    stopped++;
                }
            }
            Log.Debug($"Stopped {stopped} NAPS2 sharing server processes");
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error stopping NAPS2 server process", ex);
        }
    }
}