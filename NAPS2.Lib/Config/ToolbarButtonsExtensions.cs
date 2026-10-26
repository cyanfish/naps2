namespace NAPS2.Config;

public static class ToolbarButtonsExtensions
{
    /// <summary>
    /// Buttons that aren't shown by default, only if they're added to ExtraButtons.
    /// </summary>
    public const ToolbarButtons OptionalButtons = ToolbarButtons.None;

    public static bool IsOptional(this ToolbarButtons button) => (OptionalButtons & button) == button;

    public static bool IsToolbarButtonShown(this Naps2Config config, ToolbarButtons button) =>
        button.IsOptional()
            ? config.Get(c => c.ExtraButtons).HasFlag(button)
            : !config.Get(c => c.HiddenButtons).HasFlag(button);
}
