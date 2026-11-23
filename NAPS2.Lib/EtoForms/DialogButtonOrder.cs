namespace NAPS2.EtoForms;

/// <summary>
/// The platform's order of dialog buttons, left to right.
/// </summary>
public enum DialogButtonOrder
{
    /// <summary>
    /// OK before Cancel, e.g. "Save / Don't Save / Cancel" (Windows, KDE).
    /// </summary>
    PrimarySecondaryClose,

    /// <summary>
    /// Cancel before OK, e.g. "Don't Save / Cancel / Save" (macOS, GNOME).
    /// </summary>
    SecondaryClosePrimary
}