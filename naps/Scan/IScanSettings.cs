using System;
namespace NAPS.Scan
{
    public interface IScanSettings
    {
        IScanDevice Device { get; }
        string DisplayName { get; }
    }
}
