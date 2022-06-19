namespace NAPS2.Config;

public class FormState
{
    public string? Name { get; set; }

    public FormLocation Location { get; set; }

    public FormSize Size { get; set; }

    public bool Maximized { get; set; }

    public record FormLocation(int X, int Y);

    public record FormSize(int Width, int Height);
}