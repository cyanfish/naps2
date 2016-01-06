using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAPS2.ImportExport;
using NAPS2.Lang.Resources;
using NAPS2.Operation;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FProgress : FormBase
    {
        private readonly IScannedImageImporter scannedImageImporter;
        private readonly IErrorOutput errorOutput;

        private int currentFile = 0;
        private int currentImage = 0;
        private int imagesInFileCount = 0;
        private bool cancel;
        private bool importing;

        public FProgress(IScannedImageImporter scannedImageImporter, IErrorOutput errorOutput)
        {
            this.scannedImageImporter = scannedImageImporter;
            this.errorOutput = errorOutput;
            InitializeComponent();
        }

        public IOperation Operation { get; set; }

        public List<string> FilesToImport { get; set; }

        public Action<IScannedImage> ImageCallback { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(progressBarTop)
                    .WidthToForm()
                .Bind(btnCancel)
                    .RightToForm()
                .Activate();

            DisplayProgress();
            StartImport();
        }

        private void StartImport()
        {
            importing = true;
            Task.Factory.StartNew(() =>
            {
                foreach (var fileName in FilesToImport)
                {
                    try
                    {
                        var images = scannedImageImporter.Import(fileName, (i, j) =>
                        {
                            currentImage = i;
                            imagesInFileCount = j;
                            DisplayProgress();
                            return !cancel;
                        });
                        foreach (var img in images)
                        {
                            Invoke(new Action(() => ImageCallback(img)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.ErrorException(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(fileName)), ex);
                        errorOutput.DisplayError(string.Format(MiscResources.ImportErrorCouldNot, Path.GetFileName(fileName)));
                    }
                    currentFile++;
                    DisplayProgress();
                }
                importing = false;
                Invoke(new Action(Close));
            });
        }

        private void DisplayProgress()
        {
            Invoke(new Action(() =>
            {
                if (FilesToImport.Count == 1)
                {
                    // Display image count
                    progressBarTop.Value = currentImage;
                    progressBarTop.Maximum = imagesInFileCount;
                }
                else
                {
                    // Display file count
                    progressBarTop.Value = currentFile;
                    progressBarTop.Maximum = FilesToImport.Count;
                }
                if (progressBarTop.Maximum == 1)
                {
                    labelTop.Text = "";
                    progressBarTop.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    labelTop.Text = string.Format(MiscResources.Progress, progressBarTop.Value, progressBarTop.Maximum);
                    progressBarTop.Style = ProgressBarStyle.Continuous;
                }
                Refresh();
            }));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancel = true;
            btnCancel.Enabled = false;
        }

        private void FDownloadProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (importing)
            {
                cancel = true;
                btnCancel.Enabled = false;
                e.Cancel = true;
            }
        }
    }
}
