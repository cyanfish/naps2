using System.Runtime.InteropServices.ComTypes;

namespace NAPS2.Platform.Windows;

/// <summary>
/// Manages a user startup item on Windows.
/// </summary>
public class WindowsServiceManager : IOsServiceManager
{
    private static string ShortcutPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft\\Windows\\Start Menu\\Programs\\Startup\\NAPS2 Scanner Sharing.lnk");

    public bool CanRegister => true;

    public bool IsRegistered => File.Exists(ShortcutPath);

    public void Register()
    {
        // ReSharper disable SuspiciousTypeConversion.Global
        var shortcut = (Win32.IShellLink) new Win32.ShellLink();
        shortcut.SetWorkingDirectory(AssemblyHelper.EntryFolder);
        shortcut.SetPath(AssemblyHelper.EntryFile);
        shortcut.SetArguments("server");
        shortcut.SetIconLocation(AssemblyHelper.EntryFile, 0);
        var persistFile = (IPersistFile) shortcut;
        persistFile.Save(ShortcutPath, false);
        // TODO: Start process
    }

    public void Unregister()
    {
        File.Delete(ShortcutPath);
        // TODO: Kill running process (using ProcessCoordinator?)
    }
}