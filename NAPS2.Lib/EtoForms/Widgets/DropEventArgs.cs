using System.Collections.Immutable;

namespace NAPS2.EtoForms.Widgets;

public class DropEventArgs : EventArgs
{
    public DropEventArgs(int position, IEnumerable<string> filePaths)
    {
        Position = position;
        FilePaths = filePaths.ToImmutableList();
    }

    public DropEventArgs(int position, byte[] customData)
    {
        Position = position;
        CustomData = customData;
    }

    public int Position { get; }

    public ImmutableList<string>? FilePaths { get; }

    public byte[]? CustomData { get; }
}