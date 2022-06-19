namespace NAPS2.Util;

public class StubOverwritePrompt : OverwritePrompt
{
    public override OverwriteResponse ConfirmOverwrite(string path) => OverwriteResponse.No;
}