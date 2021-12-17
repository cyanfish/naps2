using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using EventArgs = System.EventArgs;

namespace NAPS2.Wia;

public class WiaTransfer : NativeWiaObject
{
    private const int MSG_STATUS = 1;
    private const int MSG_END_STREAM = 2;
    private const int MSG_END_TRANSFER = 3;

    private bool _cancel;

    protected internal WiaTransfer(WiaVersion version, IntPtr handle) : base(version, handle)
    {
    }

    public event EventHandler<ProgressEventArgs>? Progress;

    public event EventHandler<PageScannedEventArgs>? PageScanned;

    public event EventHandler? TransferComplete;

    public bool Download()
    {
        var hr = Version == WiaVersion.Wia10
            ? NativeWiaMethods.Download1(Handle, TransferStatusCallback)
            : NativeWiaMethods.Download2(Handle, TransferStatusCallback);
        if (hr == 1)
        {
            // User cancelled
            return false;
        }
        WiaException.Check(hr);
        return true;
    }

    public void Cancel()
    {
        _cancel = true;
    }

    private bool TransferStatusCallback(int msgType, int percent, ulong bytesTransferred, uint hresult, IStream stream)
    {
        switch (msgType)
        {
            case MSG_STATUS:
                Progress?.Invoke(this, new ProgressEventArgs(percent));
                break;
            case MSG_END_STREAM:
                var wrappedStream = new NativeStreamWrapper(stream);
                if (_cancel)
                {
                    wrappedStream.Dispose();
                }
                else
                {
                    PageScanned?.Invoke(this, new PageScannedEventArgs(wrappedStream));
                }
                break;
            case MSG_END_TRANSFER:
                TransferComplete?.Invoke(this, EventArgs.Empty);
                break;
        }
        return !_cancel;
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int percent)
        {
            Percent = percent;
        }

        public int Percent { get; }
    }

    public class PageScannedEventArgs : EventArgs
    {
        public PageScannedEventArgs(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }
    }

    public class TransferErrorEventArgs : EventArgs
    {
        public TransferErrorEventArgs(uint errorCode)
        {
            ErrorCode = errorCode;
        }

        public uint ErrorCode { get; }
    }
}