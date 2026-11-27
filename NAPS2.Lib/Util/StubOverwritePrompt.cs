namespace NAPS2.Util;

internal class StubOverwritePrompt : IOverwritePrompt
{
    public OverwriteResponse ConfirmOverwrite(string path, bool multipleFiles) => OverwriteResponse.Skip;
}