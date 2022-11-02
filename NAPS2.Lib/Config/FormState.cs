namespace NAPS2.Config;

public class FormState
{
    public string? Name { get; set; }

    public FormLocation Location { get; set; }

    public FormSize Size { get; set; }

    public bool Maximized { get; set; }

    public record struct FormLocation(int X = 0, int Y = 0);

    public record struct FormSize(int Width = 0, int Height = 0);

    public override string ToString()
    {
        return $"{{ Name: {Name}, Location: {Location.X} {Location.Y}, Size: {Size.Width} {Size.Height}, Maximized: {Maximized} }}";
    }
}