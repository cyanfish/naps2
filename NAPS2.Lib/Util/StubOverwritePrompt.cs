namespace NAPS2.Util;

internal class StubOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path) => OverwriteResponse.No;
}