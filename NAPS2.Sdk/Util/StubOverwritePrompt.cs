namespace NAPS2.Util;

public class StubOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path) => OverwriteResponse.No;
}