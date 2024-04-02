using System.Text;
using GrpcDotNetNamedPipes;
using static NAPS2.Remoting.ProcessCoordinatorService;

namespace NAPS2.Remoting;

/// <summary>
/// Manages communication and coordination between multiple NAPS2 GUI processes. Specifically:
/// - Allows sending messages to other NAPS2 processes via named pipes
/// - Allows taking the SingleInstance lock (or checking which process currently owns it)
/// This is different than the worker service - workers are owned by the parent process and are considered part of the
/// same unit. Instead, this class handles the case where the user (or a system feature like StillImage) opens NAPS2
/// twice.
/// </summary>
public class ProcessCoordinator(string basePath, string pipeNameFormat)
{
    private const string LOCK_FILE_NAME = "instance.lock";
    private const string PROC_FILE_NAME = "instance.proc";
    
    public static ProcessCoordinator CreateDefault() =>
        new(Paths.AppData, "NAPS2_PIPE_v2_{0}");

    private NamedPipeServer? _server;
    private FileStream? _instanceLock;

    private string LockFilePath => Path.Combine(basePath, LOCK_FILE_NAME);
    private string ProcFilePath => Path.Combine(basePath, PROC_FILE_NAME);

    private string GetPipeName(Process process)
    {
        return string.Format(pipeNameFormat, process.Id);
    }

    public void StartServer(ProcessCoordinatorServiceBase service)
    {
        _server = new NamedPipeServer(GetPipeName(Process.GetCurrentProcess()));
        ProcessCoordinatorService.BindService(_server.ServiceBinder, service);
        _server.Start();
    }

    public void KillServer()
    {
        _server?.Kill();
    }

    private ProcessCoordinatorServiceClient GetClient(Process recipient, int timeout) =>
        new(new NamedPipeChannel(".", GetPipeName(recipient),
            new NamedPipeChannelOptions { ConnectionTimeout = timeout }));

    private bool TrySendMessage(Process recipient, int timeout, Action<ProcessCoordinatorServiceClient> send)
    {
        var client = GetClient(recipient, timeout);
        try
        {
            send(client);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool Activate(Process recipient, int timeout) =>
        TrySendMessage(recipient, timeout, client => client.Activate(new ActivateRequest()));

    public bool CloseWindow(Process recipient, int timeout) =>
        TrySendMessage(recipient, timeout, client => client.CloseWindow(new CloseWindowRequest()));

    public bool ScanWithDevice(Process recipient, int timeout, string device) =>
        TrySendMessage(recipient, timeout,
            client => client.ScanWithDevice(new ScanWithDeviceRequest { Device = device }));

    public bool OpenFile(Process recipient, int timeout, params string[] paths)
    {
        var req = new OpenFileRequest();
        req.Path.AddRange(paths);
        return TrySendMessage(recipient, timeout, client => client.OpenFile(req));
    }

    public bool TryTakeInstanceLock()
    {
        if (_instanceLock != null)
        {
            return true;
        }
        try
        {
            _instanceLock = new FileStream(LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            using var procFile = new FileStream(ProcFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            procFile.SetLength(0);
            using var writer = new StreamWriter(procFile, Encoding.UTF8, 1024);
            writer.WriteLine(Process.GetCurrentProcess().Id);
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }

    public Process? GetProcessWithInstanceLock()
    {
        try
        {
            using var reader = new FileStream(ProcFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var id = int.Parse(new StreamReader(reader).ReadLine()?.Trim() ?? "");
            return Process.GetProcessById(id);
        }
        catch (Exception)
        {
            return null;
        }
    }
}