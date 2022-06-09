namespace NAPS2.Images.Storage;

public class StubImageMetadataFactory : IImageMetadataFactory
{
    public IImageMetadata CreateMetadata(IImageStorage storage) => new StubImageMetadata();
        
    public void CommitAllMetadata()
    {
    }
}