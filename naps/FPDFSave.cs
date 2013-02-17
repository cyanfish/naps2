using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NAPS
{
    public partial class FPDFSave : Form
    {
        private readonly IPdfExporter pdfExporter;

        public FPDFSave(IPdfExporter pdfExporter)
        {
            InitializeComponent();
            this.pdfExporter = pdfExporter;
            this.Shown += FPDFSave_Shown;
        }

        public string Filename { get; set; }

        public IList<CScannedImage> Images { get; set; }

        private void exportPDFProcess()
        {
            PdfInfo info = new PdfInfo
            {
                Title = "Scanned Image",
                Subject = "Scanned Image",
                Author = "NAPS2"
            };
            var imgs = Images.Select(x => (Image) x.GetImage()).ToList();
            pdfExporter.Export(Filename, imgs, info, num =>
            {
                Invoke(new ThreadStart(() => SetStatus(num, imgs.Count)));
                return true;
            });
            Invoke(new ThreadStart(this.Close));
        }

        void FPDFSave_Shown(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(() => exportPDFProcess())).Start();
        }

        public void SetStatus(int count, int total)
        {
            lblStatus.Text = count.ToString() + " of " + total.ToString();
            Application.DoEvents();
        }
    }

}
