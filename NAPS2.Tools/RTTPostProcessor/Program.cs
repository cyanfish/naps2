using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace NAPS2.Tools.RTTPostProcessor
{
    /// <summary>
    /// A small tool to make working with Resource Translation Toolkit (RTT) easier.
    /// The resx-files written by RTT can be post processed by this tool to exclude
    /// any entries irrelevant for translation.
    /// (c) Peter Hommel 2016
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // First of all: Close nag screens of evaluation version of Resource Translation Toolkit (RTT)
                // if found. The evaluation version saves its result resx AFTER the nag screen is closed,
                // so we have to make sure it is closed, before we can start post processing the resx.
                // A real help with evaluating RTT properly ;-P
                bool windowFound = false;
                while (CloseWindow("About Resource Translation Toolkit")) windowFound = true;
                if (windowFound)
                    Thread.Sleep(3000);

                // This is necessary to make ResXResourceReader work on non english dev systems 
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                // Now we can begin post processing the resx:
                string resxFile = args[0];
                if (File.Exists(resxFile + ".new"))
                    File.Delete(resxFile + ".new");
                var resxReader = new ResXResourceReader(resxFile);
                var resxWriter = new ResXResourceWriter(resxFile+".new");

                // Compile blacklists of types / names we want to exclude from our final resx
                var blacklistTypes = new List<Type>();
                var blacklistNames = new List<string>();

                blacklistTypes.AddRange(new []
                {
                    typeof(Size), typeof(Point), typeof(Padding), typeof(AnchorStyles),
                    typeof(Image), typeof(Bitmap), typeof(ImageLayout), typeof(Icon),
                    typeof(FormStartPosition)
                });

                blacklistNames.AddRange(new []
                {
                    "Name", "Type", "Parent", "ZOrder", "TabIndex", "AutoSize"
                });

                Console.WriteLine("Post-processing Resx: "+ resxFile);

                // Shovel each resx entry into the new file, excuding those that are blacklisted
                foreach (DictionaryEntry entry in resxReader)
                {
                    var key = entry.Key.ToString();
                    var val = entry.Value;
                    if (blacklistTypes.Contains(val.GetType()))
                        continue;

                    if (blacklistNames.Any(blname => key.Contains("." + blname)))
                        continue;

                    resxWriter.AddResource(entry.Key.ToString(),entry.Value);
                }

                // Write the result to disk and replace old file
                resxReader.Close();

                resxWriter.Generate();
                resxWriter.Close();
                resxWriter.Dispose();

                File.Delete(resxFile);
                File.Move(resxFile+".new",resxFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool CloseWindow(string windowTitle)
        {
            IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, windowTitle);
            if (windowPtr == IntPtr.Zero)
                return false;

            SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            return true;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        const UInt32 WM_CLOSE = 0x0010;
    }
}
