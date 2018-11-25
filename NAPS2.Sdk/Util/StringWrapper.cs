using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
    /// <summary>
    /// A helper class for inserting newlines to break up overly wide text.
    /// </summary>
    public class StringWrapper
    {
        public string Wrap(string text, int maxWidth, Graphics drawingGraphics, Font drawingFont)
        {
            var result = new StringBuilder();
            var parts = new Queue<string>(text.Split(' '));
            while (parts.Count > 0)
            {
                var nextParts = new List<string>();
                do
                {
                    nextParts.Add(parts.Dequeue());
                } while (
                    parts.Count > 0 && drawingGraphics.MeasureString(string.Join(" ", nextParts.Concat(new[] { parts.Peek() })).Replace("&", ""),
                        drawingFont).Width < maxWidth);
                result.Append(string.Join(" ", nextParts));
                if (parts.Count > 0)
                {
                    result.Append("\n");
                }
            }
            return result.ToString();
        }
    }
}
