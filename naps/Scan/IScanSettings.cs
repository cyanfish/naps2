using System;
namespace NAPS
{
    public interface IScanSettings
    {
        IScanDevice Device { get; }
        string DisplayName { get; }
    }
}
