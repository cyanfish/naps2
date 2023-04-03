namespace NAPS2.EtoForms.Notifications;

public class SaveNotification : LinkNotification
{
    public SaveNotification(string message, string path)
        : base(message, Path.GetFileName(path), path, Path.GetDirectoryName(path))
    {
        HideTimeout = HIDE_SHORT;
    }
}