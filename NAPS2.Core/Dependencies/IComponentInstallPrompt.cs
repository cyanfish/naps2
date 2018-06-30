namespace NAPS2.Dependencies
{
    public interface IComponentInstallPrompt
    {
        bool PromptToInstall(DownloadInfo download, ExternalComponent component, string promptText);
    }
}