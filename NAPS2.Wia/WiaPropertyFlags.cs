using System;

namespace NAPS2.Wia
{
    [Flags]
    public enum WiaPropertyFlags
    {
        Empty = 0,
        Read = 0x1,
        Write = 0x2,
        ReadWrite = Read | Write,
        None = 0x8,
        Range = 0x10,
        List = 0x20,
        Flag = 0x40,
        Cacheable = 0x10000
    }
}