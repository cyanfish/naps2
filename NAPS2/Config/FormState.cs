using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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