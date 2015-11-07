using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NAPS2.Util
{
    public static class ConditionalControls
    {
        public static void SetVisible(Control control, bool visible)
        {
            if (visible)
            {
                Show(control);
            }
            else
            {
                Hide(control);
            }
        }

        public static void Hide(Control control)
        {
            if (!control.Visible)
            {
                return;
            }
            int height = control.Height;
            int bottom = LocationInForm(control).Y + height;
            foreach (var c in EnumerateParents(control))
            {
                c.Height -= height;
            }
            foreach (var c in EnumerateSiblingsAndUncles(control))
            {
                if (LocationInForm(c).Y > bottom)
                {
                    c.Top -= height;
                }
            }
            control.Visible = false;
        }

        public static void Show(Control control)
        {
            if (control.Visible)
            {
                return;
            }
            int height = control.Height;
            int top = LocationInForm(control).Y;
            foreach (var c in EnumerateParents(control))
            {
                c.Height += height;
            }
            foreach (var c in EnumerateSiblingsAndUncles(control))
            {
                if (LocationInForm(c).Y > top)
                {
                    c.Top += height;
                }
            }
            control.Visible = true;
        }

        private static IEnumerable<Control> EnumerateParents(Control control)
        {
            for (var parent = control.Parent; parent != null; parent = parent.Parent)
            {
                yield return parent;
            }
        }

        private static IEnumerable<Control> EnumerateSiblingsAndUncles(Control control)
        {
            return EnumerateParents(control).SelectMany(x => x.Controls.Cast<Control>()).Except(EnumerateParents(control));
        }

        private static Point LocationInForm(Control control)
        {
            var x = control.Location.X;
            var y = control.Location.Y;
            foreach (var parent in EnumerateParents(control))
            {
                if (!(parent is Form))
                {
                    x += parent.Left;
                    y += parent.Top;
                }
            }
            return new Point(x, y);
        }
    }
}
