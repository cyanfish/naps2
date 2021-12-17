namespace NAPS2.Dependencies;

public interface IComponentInstallPrompt
{
    bool PromptToInstall(ExternalComponent component, string promptText);
}