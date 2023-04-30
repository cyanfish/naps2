namespace NAPS2.Scan.Internal.Sane.Native;

internal struct SaneReadParameters
{
    public SaneFrameType Frame;
    public int LastFrame;
    public int BytesPerLine;
    public int PixelsPerLine;
    public int Lines;
    public int Depth;
}