namespace NAPS2.Sdk.Tests;

public abstract class StorageConfig
{
    public abstract void Apply(ContextualTests contextualTests);
    
    public class Memory : StorageConfig
    {
        public override void Apply(ContextualTests contextualTests)
        {
        }
    }

    public class File : StorageConfig
    {
        public override void Apply(ContextualTests ctx)
        {
            var recoveryPath = Path.Combine(ctx.FolderPath, "recovery");
            ctx.ScanningContext.FileStorageManager = FileStorageManager.CreateFolder(recoveryPath);
        }
    }
}