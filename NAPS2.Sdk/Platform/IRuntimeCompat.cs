namespace NAPS2.Platform;

// TODO: Remove this class as we no longer support mono
public interface IRuntimeCompat
{
    bool UseToolStripRenderHack { get; }

    bool SetToolbarFont { get; }

    bool IsImagePaddingSupported { get; }

    bool IsToolbarTextboxSupported { get; }

    bool SetImageListSizeOnImageCollection { get; }
        
    bool UseSpaceInListViewItem { get; }

    bool RefreshListViewAfterChange { get; }

    string? ExeRunner { get; }

    bool UseWorker { get; }
}