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

        // TODO: Migrate to HttpClient
#pragma warning disable SYSLIB0014
        _client = new WebClient();
#pragma warning restore SYSLIB0014
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
            if (!VerifyHash(_tempPath!, _update!.Sha256))
            {
                Log.Error($"Update error for {_update!.Name}: hash does not match");
                _errorOutput.DisplayError(MiscResources.UpdateError);
                return;
            }
            if (!VerifySignature(_update!.Sha256, _update.Signature256))
            {
                Log.Error($"Update error for {_update!.Name}: signature does not validate");
                _errorOutput.DisplayError(MiscResources.UpdateError);
                return;
            }

#if ZIP
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
        ZipFile.ExtractToDirectory(_tempPath!, _tempFolder!);
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

    internal static bool VerifyHash(string path, byte[] expectedSha256)
    {
        using var sha = SHA256.Create();
        using FileStream stream = File.OpenRead(path);
        byte[] checksum = sha.ComputeHash(stream);
        return checksum.SequenceEqual(expectedSha256);
    }

    internal static bool VerifySignature(byte[] sha256, byte[] signature256)
    {
        var cert = X509CertificateLoader.LoadCertificate(ClientCreds_.naps2_public);
        var csp = cert.GetRSAPublicKey();
        if (csp == null) return false;
        try
        {
            return csp.VerifyHash(sha256, signature256, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private void DownloadProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        Status.CurrentProgress = (int) e.BytesReceived;
        Status.MaxProgress = (int) e.TotalBytesToReceive;
        InvokeStatusChanged();
    }
}