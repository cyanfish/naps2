namespace NAPS2.Config;

public class FormState
{
    public string? Name { get; set; }

    public FormLocation Location { get; set; }

    public FormSize Size { get; set; }

    public bool Maximized { get; set; }

    public class FormLocation
    {
        public FormLocation()
        {
        }

        public FormLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public int X { get; set; }
        public int Y { get; set; }
    }
    
    public class FormSize
    {
        public FormSize()
        {
        }
        
        public FormSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        public int Width { get; set; }
        public int Height { get; set; }
    }
}