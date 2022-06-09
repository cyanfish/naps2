namespace NAPS2.Images.Storage;

public interface IImageMetadataFactory
{
    IImageMetadata CreateMetadata(IImageStorage storage);
        
    void CommitAllMetadata();
}