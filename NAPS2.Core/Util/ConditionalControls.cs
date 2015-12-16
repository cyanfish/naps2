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

        public static void Hide(Control control, int margin = 0)
        {
            if (!control.Visible)
            {
                return;
            }
            int height = control.Height + margin;
            int bottom = LocationInForm(control).Y + height;
            foreach (var c in EnumerateParents(control))
            {
                c.Height -= height;
            }
            foreach (var c in EnumerateSiblingsAndUncles(control))
            {
                if (LocationInForm(c).Y >= bottom)
                {
                    c.Top -= height;
                }
            }
            control.Visible = false;
        }

        public static void Show(Control control, int margin = 0)
        {
            if (control.Visible)
            {
                return;
            }
            int height = control.Height + margin;
            int top = LocationInForm(control).Y;
            foreach (var c in EnumerateParents(control))
            {
                c.Height += height;
            }
            foreach (var c in EnumerateSiblingsAndUncles(control))
            {
                if (LocationInForm(c).Y >= top)
                {
                    c.Top += height;
                }
            }
            control.Visible = true;
        }

        public static void LockHeight(Form form)
        {
            form.MaximumSize = new Size(int.MaxValue, form.Height);
            form.MinimumSize = new Size(0, form.Height);
        }

        public static void UnlockHeight(Form form)
        {
            form.MaximumSize = new Size(0, 0);
            form.MinimumSize = new Size(0, 0);
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
            var parentsAndSelf = EnumerateParents(control).Concat(Enumerable.Repeat(control, 1));
            return EnumerateParents(control).SelectMany(x => x.Controls.Cast<Control>()).Except(parentsAndSelf);
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
