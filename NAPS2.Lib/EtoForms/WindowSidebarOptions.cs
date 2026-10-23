namespace NAPS2.EtoForms;

/// <summary>
/// Options for a native window sidebar (see EtoPlatform.AttachWindowSidebar).
/// </summary>
public record WindowSidebarOptions
{
    public required int Width { get; init; }

    public int? MinWidth { get; init; }

    public int? MaxWidth { get; init; }

    public int? MinContentWidth { get; init; }

    public bool CanHide { get; init; }

    public bool Visible { get; init; } = true;

    public bool ContentUnderTitleBar { get; init; }

    public Action<int>? WidthChanged { get; init; }

    public Action<bool>? VisibleChanged { get; init; }
}
