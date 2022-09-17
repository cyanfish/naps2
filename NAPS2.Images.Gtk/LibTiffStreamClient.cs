using System.Runtime.InteropServices;
using toff_t = System.IntPtr;
using tsize_t = System.IntPtr;
using thandle_t = System.IntPtr;
using tdata_t = System.IntPtr;

namespace NAPS2.Images.Gtk;

public class LibTiffStreamClient
{
    private readonly Stream _stream;
    private readonly LibTiff.TIFFErrorHandler _error;
    private readonly LibTiff.TIFFErrorHandler _warning;
    private readonly LibTiff.TIFFReadWriteProc _read;
    private readonly LibTiff.TIFFReadWriteProc _write;
    private readonly LibTiff.TIFFSeekProc _seek;
    private readonly LibTiff.TIFFCloseProc _close;
    private readonly LibTiff.TIFFSizeProc _size;
    private readonly LibTiff.TIFFMapFileProc _map;
    private readonly LibTiff.TIFFUnmapFileProc _unmap;

    public LibTiffStreamClient(Stream stream)
    {
        _stream = stream;
        // We need to keep explicit references to the delegates to avoid garbage collection
        _error = Error;
        _warning = Warning;
        _read = Read;
        _write = Write;
        _seek = Seek;
        _close = Close;
        _size = Size;
        _map = Map;
        _unmap = UnMap;
    }

    public IntPtr TIFFClientOpen(string mode)
    {
        LibTiff.TIFFSetErrorHandler(_error);
        LibTiff.TIFFSetWarningHandler(_warning);
        return LibTiff.TIFFClientOpen("placeholder", mode, IntPtr.Zero,
            _read, _write, _seek, _close, _size, _map, _unmap);
    }

    private void Warning(string x, string y, IntPtr va_args)
    {
    }

    private void Error(string x, string y, IntPtr va_args)
    {
    }

    public tsize_t Read(thandle_t clientdata, tdata_t data, tsize_t size)
    {
        var buffer = new byte[(int) size];
        var count = _stream.Read(buffer);
        Marshal.Copy(buffer, 0, data, count);
        return (tsize_t) count;
    }

    public tsize_t Write(thandle_t clientdata, tdata_t data, tsize_t size)
    {
        var buffer = new byte[(int) size];
        Marshal.Copy(data, buffer, 0, buffer.Length);
        _stream.Write(buffer);
        return (tsize_t) buffer.Length;
    }

    public toff_t Seek(thandle_t clientdata, toff_t off, int c)
    {
        if (c == 0)
        {
            _stream.Seek((long) off, SeekOrigin.Begin);
        }
        if (c == 1)
        {
            _stream.Seek((long) off, SeekOrigin.Current);
        }
        if (c == 2)
        {
            _stream.Seek((long) off, SeekOrigin.End);
        }
        return (toff_t) _stream.Position;
    }

    public int Close(thandle_t clientdata)
    {
        return 0;
    }

    public toff_t Size(thandle_t clientdata)
    {
        return (toff_t) _stream.Length;
    }

    public int Map(thandle_t clientdata, ref tdata_t a, ref toff_t b)
    {
        return 0;
    }

    public void UnMap(thandle_t clientdata, tdata_t a, toff_t b)
    {
    }
}