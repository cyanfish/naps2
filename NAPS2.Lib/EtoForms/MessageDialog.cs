using Eto.Forms;

namespace NAPS2.EtoForms;

/// <summary>
/// Shows message dialogs with contextual button text. Each platform shows its native (or native-looking)
/// dialog and orders the buttons by its own conventions.
/// </summary>
public static class MessageDialog
{
    /// <summary>
    /// Shows a message in the owner window (e.g. as a sheet on macOS).
    /// </summary>
    public static MessageDialogResult Show(Window owner, MessageDialogOptions options) =>
        EtoPlatform.Current.ShowMessageDialog(owner, options);

    /// <summary>
    /// Shows an app-level message that isn't tied to a particular window.
    /// </summary>
    public static MessageDialogResult Show(MessageDialogOptions options) =>
        EtoPlatform.Current.ShowMessageDialog(null, options);

    /// <summary>
    /// Shows a message with an OK button.
    /// </summary>
    public static void Show(Window owner, string text, string title, MessageBoxType type) =>
        Show(owner, OkOptions(text, title, type));

    /// <summary>
    /// Shows an app-level message with an OK button.
    /// </summary>
    public static void Show(string text, string title, MessageBoxType type) =>
        Show(OkOptions(text, title, type));

    /// <summary>
    /// Asks for confirmation of an action, with a button for the action (e.g. "Delete") and a Cancel button.
    /// </summary>
    public static bool Confirm(Window owner, string text, string title, string actionText,
        MessageBoxType type = MessageBoxType.Question, bool destructive = false) =>
        Show(owner, new MessageDialogOptions
        {
            Title = title,
            Text = text,
            Type = type,
            PrimaryText = actionText,
            Destructive = destructive
        }) == MessageDialogResult.Primary;

    private static MessageDialogOptions OkOptions(string text, string title, MessageBoxType type) =>
        new() { Title = title, Text = text, Type = type, CloseText = UiStrings.OK };
}

public enum MessageDialogResult
{
    Close,
    Primary,
    Secondary
}

public record MessageDialogOptions
{
    public required string Title { get; init; }
    public required string Text { get; init; }
    public MessageBoxType Type { get; init; } = MessageBoxType.Information;
    public string? PrimaryText { get; init; }
    public string? SecondaryText { get; init; }
    public string CloseText { get; init; } = UiStrings.Cancel;
    public MessageDialogResult DefaultButton { get; init; } = MessageDialogResult.Primary;

    /// <summary>
    /// Whether the primary action is destructive (e.g. deleting), for platforms that style those differently.
    /// </summary>
    public bool Destructive { get; init; }

    /// <summary>
    /// The buttons in the given order, without the ones that have no text.
    /// </summary>
    /// <param name="order">The platform's left-to-right order.</param>
    /// <param name="rtlOrder">Whether to return the buttons right to left (e.g. for NSAlert).</param>
    public IReadOnlyList<(string Text, MessageDialogResult Result)> GetButtons(DialogButtonOrder order,
        bool rtlOrder = false)
    {
        MessageDialogResult[] results = order switch
        {
            DialogButtonOrder.PrimarySecondaryClose =>
                [MessageDialogResult.Primary, MessageDialogResult.Secondary, MessageDialogResult.Close],
            DialogButtonOrder.SecondaryClosePrimary =>
                [MessageDialogResult.Secondary, MessageDialogResult.Close, MessageDialogResult.Primary],
            _ => throw new ArgumentException()
        };
        if (rtlOrder) results = results.Reverse().ToArray();
        return results.Where(x => GetText(x) != null).Select(x => (GetText(x)!, x)).ToList();
    }

    private string? GetText(MessageDialogResult result) => result switch
    {
        MessageDialogResult.Primary => PrimaryText,
        MessageDialogResult.Secondary => SecondaryText,
        _ => CloseText
    };

    /// <summary>
    /// The default button, falling back to the close button if the specified one has no text.
    /// </summary>
    public MessageDialogResult EffectiveDefaultButton =>
        GetText(DefaultButton) != null ? DefaultButton : MessageDialogResult.Close;
}