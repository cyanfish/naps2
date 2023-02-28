namespace NAPS2.Scan.Internal.Sane.Native;

public interface ISaneDevice
{
    void Cancel();
    void Start();
    SaneReadParameters GetParameters();
    bool Read(byte[] buffer, out int len);
}