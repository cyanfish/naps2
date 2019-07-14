using System;

namespace NAPS2.Config
{
    [Flags]
    public enum ToolbarButtons
    {
        None,
        All = 0x1FFFF,
        Scan = 1 << 1,
        Profiles = 1 << 2,
        Ocr = 1 << 3,
        Import = 1 << 4,
        SavePdf = 1 << 5,
        SaveImages = 1 << 6,
        EmailPdf = 1 << 7,
        Print = 1 << 8,
        Image = 1 << 9,
        Rotate = 1 << 10,
        Move = 1 << 11,
        Reorder = 1 << 12,
        Delete = 1 << 13,
        Clear = 1 << 14,
        Language = 1 << 15,
        Settings = 1 << 16,
        About = 1 << 17,
        Donate = 1 << 18
    }
}
