namespace NAPS2.Config;

[Flags]
public enum ToolbarButtons
{
    None,
    All = 0x1FFFF,
    Scan = 1 << 1,
    Profiles = 1 << 2,
    Ocr = 1 << 3,
    Import = 1 << 4,
    Squeeze = 1 << 5,
    SavePdf = 1 << 6,
    SaveImages = 1 << 7,
    EmailPdf = 1 << 8,
    Print = 1 << 9,
    Image = 1 << 10,
    Rotate = 1 << 11,
    Move = 1 << 12,
    Reorder = 1 << 13,
    Delete = 1 << 14,
    Clear = 1 << 15,
    Language = 1 << 16,
    Settings = 1 << 17,
    About = 1 << 18,
    Donate = 1 << 19
}