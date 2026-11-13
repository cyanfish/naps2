namespace NAPS2.Images.Skia;

/// <summary>
/// A write-only stream that passes encoded image data through to another stream, adding resolution metadata to the
/// file header along the way. This lets Skia encode directly to the output without the full encoded image needing to
/// be held in memory.
/// </summary>
internal class SkiaResolutionStream : Stream
{
    private readonly Stream _output;
    private readonly ImageFileFormat _format;
    private readonly float _xDpi;
    private readonly float _yDpi;
    private byte[]? _header;
    private int _headerLength;

    public SkiaResolutionStream(Stream output, ImageFileFormat format, float xDpi, float yDpi)
    {
        _output = output;
        _format = format;
        _xDpi = xDpi;
        _yDpi = yDpi;
        var headerLength = SkiaMetadataHelper.GetHeaderLength(format);
        if (headerLength > 0)
        {
            _header = new byte[headerLength];
        }
    }

    public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_header != null)
        {
            // Hold back the start of the file until we have the full header
            var headerBytes = Math.Min(buffer.Length, _header.Length - _headerLength);
            buffer.Slice(0, headerBytes).CopyTo(_header.AsSpan(_headerLength));
            _headerLength += headerBytes;
            buffer = buffer.Slice(headerBytes);
            if (_headerLength < _header.Length)
            {
                return;
            }
            var headerWithResolution = SkiaMetadataHelper.WriteResolution(_header, _format, _xDpi, _yDpi);
            _output.Write(headerWithResolution, 0, headerWithResolution.Length);
            _header = null;
        }
        _output.Write(buffer);
    }

    /// <summary>
    /// Writes any held-back data to the output. This must be called once encoding is done.
    /// </summary>
    public void Complete()
    {
        if (_header != null)
        {
            // The data was shorter than the expected header, so it can't be patched
            _output.Write(_header, 0, _headerLength);
            _header = null;
        }
    }

    public override void Flush()
    {
        // The underlying stream isn't flushed until the header has been written, which happens in order anyway
        if (_header == null)
        {
            _output.Flush();
        }
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
