namespace NAPS2.EtoForms;

public class StubDialogHelper : DialogHelper
{
    public override bool PromptToSavePdfOrImage(string? defaultPath, out string? savePath)
    {
        savePath = null;
        return false;
    }

    public override bool PromptToSavePdf(string? defaultPath, out string? savePath)
    {
        savePath = null;
        return false;
    }

    public override bool PromptToSaveImage(string? defaultPath, out string? savePath)
    {
        savePath = null;
        return false;
    }

    public override bool PromptToImport(out string[]? filePaths)
    {
        filePaths = null;
        return false;
    }

    public override bool PromptToSelectFolder(string? folderPath, out string? savePath)
    {
        savePath = null;
        return false;
    }
}