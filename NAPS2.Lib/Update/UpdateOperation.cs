using System.ComponentModel;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using NAPS2.EtoForms.Desktop;

namespace NAPS2.Update;

public class UpdateOperation : OperationBase
{
    private readonly ErrorOutput _errorOutput;
    private readonly DesktopController _desktopController;
    private readonly DesktopFormProvider _desktopFormProvider;

    private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);
    private WebClient? _client;
    private UpdateInfo? _update;
    private string? _tempFolder;
    private string? _tempPath;

    static UpdateOperation()
    {
        try
        {
            const int tls13 = 12288;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType) tls13;
        }
        catch (NotSupportedException)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }
    }

    public UpdateOperation(ErrorOutput errorOutput, DesktopController desktopController,
        DesktopFormProvider desktopFormProvider)
    {
        _errorOutput = errorOutput;
        _desktopController = desktopController;
        _desktopFormProvider = desktopFormProvider;

        ProgressTitle = MiscResources.UpdateProgress;
        AllowBackground = true;
        AllowCancel = true;
        Status = new OperationStatus
        {
            StatusText = MiscResources.Updating,
            ProgressType = OperationProgressType.MB
        };
    }

    public void Start(UpdateInfo updateInfo)
    {
        _update = updateInfo;
        _tempFolder = Path.Combine(Paths.Temp, Path.GetRandomFileName());
        Directory.CreateDirectory(_tempFolder);
        _tempPath = Path.Combine(_tempFolder,
            updateInfo.DownloadUrl.Substring(updateInfo.DownloadUrl.LastIndexOf('/') + 1));

        _client = new WebClient();
        _client.DownloadProgressChanged += DownloadProgress;
        _client.DownloadFileCompleted += DownloadCompleted;
        _client.DownloadFileAsync(new Uri(updateInfo.DownloadUrl), _tempPath);
    }

    public override void Cancel()
    {
        _client?.CancelAsync();
    }

    public override void Wait(CancellationToken cancelToken = default)
    {
        while (!_waitHandle.WaitOne(1000) && !cancelToken.IsCancellationRequested)
        {
        }
    }

    private void DownloadCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        try
        {
            if (e.Cancelled)
            {
                return;
            }
            if (e.Error != null)
            {
                e.Error.PreserveStackTrace();
                throw e.Error;
            }
            if (!VerifyHash())
            {
                Log.Error($"Update error for {_update!.Name}: hash does not match");
                _errorOutput.DisplayError(MiscResources.UpdateError);
                return;
            }
            if (!VerifySignature())
            {
                Log.Error($"Update error for {_update!.Name}: signature does not validate");
                _errorOutput.DisplayError(MiscResources.UpdateError);
                return;
            }

#if STANDALONE
                InstallZip();
#else
            InstallExe();
#endif
        }
        catch (Exception ex)
        {
            Log.ErrorException("Update error", ex);
            _errorOutput.DisplayError(MiscResources.UpdateError);
            return;
        }
        finally
        {
            InvokeFinished();
            _waitHandle.Set();
        }
        _desktopController.SkipRecoveryCleanup = true;
        _desktopFormProvider.DesktopForm.Close();
    }

    private void InstallExe()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _tempPath,
            Arguments = "/SILENT /CLOSEAPPLICATIONS"
        });
    }

    private void InstallZip()
    {
        ZipFile.ExtractToDirectory(_tempPath, _tempFolder);
        string portableLauncherPath = Path.Combine(AssemblyHelper.LibFolder, "..", "..", "NAPS2.Portable.exe");
        AtomicReplaceFile(Path.Combine(_tempFolder!, "NAPS2.Portable.exe"), portableLauncherPath);
        Process.Start(new ProcessStartInfo
        {
            FileName = portableLauncherPath,
            Arguments = $"/Update {Process.GetCurrentProcess().Id} \"{Path.Combine(_tempFolder!, "App")}\""
        });
    }

    private void AtomicReplaceFile(string source, string dest)
    {
        if (!File.Exists(dest))
        {
            File.Move(source, dest);
            return;
        }
        string temp = dest + ".old";
        File.Move(dest, temp);
        try
        {
            File.Move(source, dest);
            File.Delete(temp);
        }
        catch (Exception)
        {
            File.Move(temp, dest);
            throw;
        }
    }

    private bool VerifyHash()
    {
        using var sha = new SHA1CryptoServiceProvider();
        using FileStream stream = File.OpenRead(_tempPath!);
        byte[] checksum = sha.ComputeHash(stream);
        return checksum.SequenceEqual(_update!.Sha1);
    }

    private bool VerifySignature()
    {
        var cert = new X509Certificate2(ClientCreds_.naps2_public);
        var csp = (RSACryptoServiceProvider) cert.PublicKey.Key;
        return csp.VerifyHash(_update!.Sha1, CryptoConfig.MapNameToOID("SHA1"), _update.Signature);
    }

    private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        Status.CurrentProgress = (int) e.BytesReceived;
        Status.MaxProgress = (int) e.TotalBytesToReceive;
        InvokeStatusChanged();
    }
}