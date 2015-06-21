/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2014  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using PdfSharp.Pdf.Printing;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpPrinter : IPdfPrinter
    {
        public void Print(string pdfFilePath)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = false,
                Verb = "print",
                FileName = pdfFilePath
            });
            if (p != null)
            {
                p.Start();
                p.WaitForExit();
            }
            //var adobePath =
            //    Registry.LocalMachine.OpenSubKey("Software")
            //        .OpenSubKey("Microsoft")
            //        .OpenSubKey("Windows")
            //        .OpenSubKey("CurrentVersion")
            //        .OpenSubKey("App Paths")
            //        .OpenSubKey("AcroRd32.exe");
            //if (adobePath == null)
            //{
            //    MessageBox.Show("Adobe Reader must be installed in order to print.");
            //    return;
            //}
            //PdfFilePrinter.AdobeReaderPath = Path.Combine(adobePath.GetValue("Path").ToString(), "AcroRd32.exe");
            //var printDialog = new PrintDialog();
            //printDialog.AllowSelection = true;
            //if (printDialog.ShowDialog() == DialogResult.OK)
            //{
            //    var printer = new PdfFilePrinter(pdfFilePath, printDialog.PrinterSettings.PrinterName);
            //    printer.Print();
            //}
        }
    }
}
