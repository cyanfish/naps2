namespace NAPS2.Images.Storage;

public class NotToBeUsedStorageManager : FileStorageManager
{
    public override string NextFilePath() => throw new InvalidOperationException("Recovery folder hasn't been initialized");

    public NotToBeUsedStorageManager() : base(String.Empty)
    {
    }
}