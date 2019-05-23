using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.Util
{
    /// <summary>
    /// Helpers for conditionally visible controls that use simple heuristics help maintain the visual appearance of forms.
    ///
    /// For example, if a checkbox is hidden, the form will shrink and controls further down will be moved up to fill the empty space.
    /// </summary>
    public static class ConditionalControls
    {
        public static void SetVisible(Control control, bool visible, int margin = 0)
        {
            if (visible)
            {
                Show(control, margin);
            }
            else
            {
                Hide(control, margin);
            }
        }

        public static void Hide(Control control, int margin = 0)
        {
            if (!control.Visible)
            {
                return;
            }
            var bottomAnchorControls = FindAndRemoveBottomAnchor(control.FindForm());
            int height = control.Height + margin;
            int bottom = LocationInForm(control).Y + control.Height;
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
            AddBottomAnchor(bottomAnchorControls);
        }

        public static void Show(Control control, int margin = 0)
        {
            if (control.Visible)
            {
                return;
            }
            var bottomAnchorControls = FindAndRemoveBottomAnchor(control.FindForm());
            int height = control.Height + margin;
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
            AddBottomAnchor(bottomAnchorControls);
        }

        public static void LockHeight(Form form)
        {
            form.MaximumSize = new Size(int.MaxValue, form.Height);
            form.MinimumSize = new Size(form.MinimumSize.Width, form.Height);
        }

        public static void UnlockHeight(Form form)
        {
            form.MaximumSize = new Size(0, 0);
            form.MinimumSize = new Size(form.MinimumSize.Width, 0);
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

        private static IEnumerable<Control> EnumerateDescendents(Control control)
        {
            var children = control.Controls.Cast<Control>().ToList();
            return children.Concat(children.SelectMany(EnumerateDescendents));
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

        private static List<(Control, AnchorStyles)> FindAndRemoveBottomAnchor(Form form)
        {
            var controls = EnumerateDescendents(form).Where(x => (x.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom).Select(c => (c, c.Anchor)).ToList();
            foreach (var (c, a) in controls)
            {
                c.Anchor = a & ~AnchorStyles.Bottom | AnchorStyles.Top;
            }
            return controls;
        }

        private static void AddBottomAnchor(List<(Control, AnchorStyles)> controls)
        {
            foreach (var (c, a) in controls)
            {
                c.Anchor = a;
            }
        }
    }
}
