using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

internal interface ITwainEvents
{
    void PageStart(TwainPageStart pageStart);

    void NativeImageTransferred(TwainNativeImage nativeImage);

    void MemoryBufferTransferred(TwainMemoryBuffer memoryBuffer);

    void TransferCanceled(TwainTransferCanceled transferCanceled);
}