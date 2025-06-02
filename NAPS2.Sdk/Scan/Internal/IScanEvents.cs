namespace NAPS2.Scan.Internal;

internal interface IScanEvents
{
    // This only includes events that can't be otherwise inferred.
    void PageStart();
    void PageProgress(double progress);
    void DeviceUriChanged(string? iconUri, string? connectionUri);
}