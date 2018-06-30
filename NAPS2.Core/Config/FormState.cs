using System.Drawing;

namespace NAPS2.Config
{
    public class FormState
    {
        public string Name { get; set; }

        public Point Location { get; set; }

        public Size Size { get; set; }

        public bool Maximized { get; set; }
    }
}