using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NAPS2.Util
{
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
            //int lastSpace = -1;
            //int lastBreak = 0;
            //while (true)
            //{
            //    int nextSpace = text.IndexOf(" ", lastBreak, StringComparison.Ordinal);
            //    if (nextSpace == -1)
            //    {
            //        nextSpace = text.Length;
            //    }
            //    string nextPiece = text.Substring(lastBreak, nextSpace - lastBreak);
            //    if (drawingGraphics.MeasureString(nextPiece, drawingFont).Width > maxWidth && lastSpace != -1)
            //    {
            //        nextPiece = text.Substring(lastBreak, lastSpace - lastBreak);
            //        result.Append(nextPiece);
            //        lastSpace = -1;
            //        lastBreak = 
            //    }
            //    else
            //    {

            //    }
            //}
        }
    }
}
