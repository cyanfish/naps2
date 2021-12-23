namespace NAPS2.Images.Storage;

public class StubImageMetadataFactory : IImageMetadataFactory
{
    public IImageMetadata CreateMetadata(IStorage storage) => new StubImageMetadata();
        
    public void CommitAllMetadata()
    {
    }
}