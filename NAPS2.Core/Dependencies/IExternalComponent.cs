namespace NAPS2.Dependencies
{
    public interface IExternalComponent
    {
        string Path { get; }

        string DataPath { get; }

        bool IsInstalled { get; }

        bool IsSupported { get; }
    }
}