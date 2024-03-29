using System.Runtime.InteropServices;

namespace NAPS2.ImportExport.Email.Mapi;

// Disable uninitailized warnings due to matching the native struct layout
#pragma warning disable CS0649
// ReSharper disable NotAccessedField.Global

/// <summary>
/// A MAPI structure describing an email sender or recipient.
/// Documented at: http://msdn.microsoft.com/en-us/library/windows/desktop/dd296720%28v=vs.85%29.aspx
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
internal class MapiRecipDesc
{
    public int reserved;
    public MapiRecipClass recipClass;
    public string name = null!;
    public string address = null!;
    public int entryIdSize;
    public IntPtr entryId;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal class MapiRecipDescW
{
    public int reserved;
    public MapiRecipClass recipClass;
    public string name = null!;
    public string address = null!;
    public int entryIdSize;
    public IntPtr entryId;
}