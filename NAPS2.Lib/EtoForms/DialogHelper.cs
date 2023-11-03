namespace NAPS2.EtoForms;

public abstract class DialogHelper
{
    public abstract bool PromptToSavePdfOrImage(string? defaultPath, out string? savePath);

    public abstract bool PromptToSavePdf(string? defaultPath, out string? savePath);

    public abstract bool PromptToSaveImage(string? defaultPath, out string? savePath);

    public abstract bool PromptToImport(out string[]? filePaths);

    public abstract bool PromptToSelectFolder(string? folderPath, out string? savePath);
}