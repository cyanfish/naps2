namespace NAPS2.Escl.Client;

public class RawDocument
{
    public required byte[] Data { get; init; }

    public required string? ContentType { get; init; }
}