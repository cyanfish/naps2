using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NAPS
{
    public partial class FPDFSave : Form
    {
        public FPDFSave()
        {
            InitializeComponent();

            this.Shown += FPDFSave_Shown;
        }

        public string Filename { get; set; }

        public IList<CScannedImage> Images { get; set; }

        private void exportPDFProcess()
        {
            PdfDocument document = new PdfDocument();
            document.Layout = PdfSharp.Pdf.IO.PdfWriterLayout.Compact;
            document.Info.Title = "Scanned Image";
            document.Info.Subject = "Scanned Image";
            document.Info.Author = "NAPS2";
            int i = 1;
            foreach (CScannedImage img in Images)
            {
                ThreadStart setstatus = delegate { this.SetStatus(i, Images.Count); };
                this.Invoke(setstatus);

                using (Bitmap baseImage = img.GetImage())
                {
                    double realWidth = baseImage.Width / baseImage.HorizontalResolution * 72;
                    double realHeight = baseImage.Height / baseImage.VerticalResolution * 72;
                    PdfPage newPage = document.AddPage();
                    newPage.Width = (int)realWidth;
                    newPage.Height = (int)realHeight;
                    XGraphics gfx = XGraphics.FromPdfPage(newPage);
                    gfx.DrawImage(baseImage, 0, 0, (int)realWidth, (int)realHeight);
                }
                i++;
            }
            document.Save(Filename);
            this.Invoke(new ThreadStart(this.Close));
        }

        void FPDFSave_Shown(object sender, EventArgs e)
        {
            ThreadStart starter = delegate { exportPDFProcess(); };
            new Thread(starter).Start();
        }

        public void SetStatus(int count, int total)
        {
            lblStatus.Text = count.ToString() + " of " + total.ToString();
            Application.DoEvents();
        }
    }

}
