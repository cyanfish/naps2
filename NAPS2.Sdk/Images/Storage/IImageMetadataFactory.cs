namespace NAPS2.Images.Storage
{
    public interface IImageMetadataFactory
    {
        IImageMetadata CreateMetadata(IStorage storage);
    }
}
