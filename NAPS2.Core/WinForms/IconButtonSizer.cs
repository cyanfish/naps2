using System;
using System.Linq;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    public class IconButtonSizer
    {
        private static int ButtonTextWidth(Button x)
        {
            int oldWidth = x.Width;
            x.AutoSize = true;
            x.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            int width = x.Width;
            x.AutoSize = false;
            x.Width = oldWidth;
            return width;
        }

        public void ResizeButtons(params Button[] buttons)
        {
            // Dynamically determine the size and padding of the add/edit/delete buttons to make localization simpler
            var maxTextWidth = buttons.Select(ButtonTextWidth).Max();
            var buttonWidth = maxTextWidth + WidthOffset; // Fixed offset based on icon width and ideal padding
            foreach (var Btn in buttons)
            {
                if (MaxWidth != 0 && buttonWidth > MaxWidth)
                {
                    // Set the button to be at least its necessary size (for sure), and at most the specified MaxWidth (preferably)
                    Btn.Width = Math.Max(MaxWidth, ButtonTextWidth(Btn) + WidthOffset);
                }
                else
                {
                    // Set the button to be the same width as the largest button
                    Btn.Width = buttonWidth;
                }
                // Update the padding so that the text center is in the same place on each button
                int rightPadding = PaddingRight + ((Btn.Width - WidthOffset - ButtonTextWidth(Btn)) / 2);
                Btn.Padding = new Padding(Btn.Padding.Left, Btn.Padding.Top, rightPadding, Btn.Padding.Bottom);
            }
        }

        public int WidthOffset { get; set; }

        public int PaddingRight { get; set; }

        public int MaxWidth { get; set; }
    }
}