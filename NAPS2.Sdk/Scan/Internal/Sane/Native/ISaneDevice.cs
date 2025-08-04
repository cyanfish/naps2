namespace NAPS2.Scan.Internal.Sane.Native;

internal interface ISaneDevice
{
    void Cancel();
    void Start();
    SaneReadParameters GetParameters();
    bool Read(byte[] buffer, out int len);
    IEnumerable<SaneOption> GetOptions();
    void SetOption(SaneOption option, bool value, out SaneOptionSetInfo info);
    void SetOption(SaneOption option, double value, out SaneOptionSetInfo info);
    void SetOption(SaneOption option, string value, out SaneOptionSetInfo info);
    void GetOption(SaneOption option, out double value);
}