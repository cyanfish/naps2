using NAPS2.Remoting;

namespace NAPS2.Platform;

/// <summary>
/// A class to help manage the lifecycle of the NAPS2 GUI.
/// </summary>
public abstract class ApplicationLifecycle
{
    private readonly ProcessCoordinator _processCoordinator;
    private readonly Naps2Config _config;

    protected ApplicationLifecycle(ProcessCoordinator processCoordinator, Naps2Config config)
    {
        _processCoordinator = processCoordinator;
        _config = config;
    }

    public virtual void ParseArgs(string[] args)
    {
    }

    public virtual void ExitIfRedundant()
    {
        HandleSingleInstance();
    }

    protected virtual void HandleSingleInstance()
    {
        // Only start one instance if configured for SingleInstance
        Log.Debug("HandleSingleInstance");
        if (_config.Get(c => c.SingleInstance))
        {
            Log.Debug("SingleInstance enabled");
            if (!_processCoordinator.TryTakeInstanceLock())
            {
                Log.Debug("Failed to get SingleInstance lock");
                var process = _processCoordinator.GetProcessWithInstanceLock();
                if (process != null)
                {
                    // Another instance of NAPS2 is running, so send it the "Activate" signal
                    Log.Debug($"Activating process {process.Id}");

                    // For new processes, wait until the process is at least 5 seconds old.
                    // This might be useful in cases where multiple NAPS2 processes are started at once, e.g. clicking
                    // to open a group of files associated with NAPS2.
                    int processAge = (DateTime.Now - process.StartTime).Milliseconds;
                    int timeout = (5000 - processAge).Clamp(100, 5000);

                    SetMainWindowToForeground(process);
                    bool ok = true;
                    if (Environment.GetCommandLineArgs() is [_, var arg] && File.Exists(arg))
                    {
                        Log.Debug($"Sending OpenFileRequest for {arg}");
                        ok = _processCoordinator.OpenFile(process, timeout, arg);
                    }
                    if (ok && _processCoordinator.Activate(process, timeout))
                    {
                        // Successful, so this instance should be closed
                        Environment.Exit(0);
                    }
                }
            }
        }
    }

    protected virtual void SetMainWindowToForeground(Process process)
    {
    }
}