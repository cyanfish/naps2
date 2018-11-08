using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using EventArgs = System.EventArgs;

namespace NAPS2.Scan.Wia.Native
{
    public class WiaTransfer : NativeWiaObject
    {
        private const int MSG_STATUS = 1;
        private const int MSG_END_STREAM = 2;
        private const int MSG_END_TRANSFER = 3;

        protected internal WiaTransfer(IntPtr handle) : base(handle)
        {
        }

        public event EventHandler<ProgressEventArgs> Progress;

        public event EventHandler<PageScannedEventArgs> PageScanned;

        public event EventHandler TransferComplete;

        public event EventHandler<TransferErrorEventArgs> TransferError;

        public void Download()
        {
            WiaException.Check(NativeWiaMethods.Download(Handle, 0, TransferStatusCallback));
        }

        public void Cancel()
        {
            WiaException.Check(NativeWiaMethods.CancelTransfer(Handle));
        }

        private void TransferStatusCallback(int msgType, int percent, ulong bytesTransferred, uint hresult, IStream stream)
        {
            if (hresult != 0)
            {
                TransferError?.Invoke(this, new TransferErrorEventArgs(hresult));
                return;
            }
            switch (msgType)
            {
                case MSG_STATUS:
                    Progress?.Invoke(this, new ProgressEventArgs(percent));
                    break;
                case MSG_END_STREAM:
                    PageScanned?.Invoke(this, new PageScannedEventArgs(new NativeStreamWrapper(stream)));
                    break;
                case MSG_END_TRANSFER:
                    TransferComplete?.Invoke(this, EventArgs.Empty);
                    break;
            }
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
}