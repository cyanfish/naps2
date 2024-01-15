namespace NAPS2.Recovery;

public class RecoveryParams
{
    // In case the user has the "Keep images across sessions" option, we want to make the recovery operation feel more
    // seamless. This means a few things:
    // - Image files are moved instead of copied. This is destructive (higher risk of data loss) but fast.
    // - Thumbnails rendering is deferred.
    // - No operation progress is displayed.
    // - The operation is run synchronously (with only moving files + no thumbnail rendering it should be trivially fast).
    // - Images are sent back to the UI as a single batch (speeds up UI rendering).
    public bool AutoSessionRestore { get; set; }

    public int? ThumbnailSize { get; set; }
}