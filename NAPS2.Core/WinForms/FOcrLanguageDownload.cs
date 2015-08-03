using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;

namespace NAPS2.WinForms
{
    public partial class FOcrLanguageDownload : FormBase
    {
        private static readonly string DownloadBase = @"https://sourceforge.net/projects/naps2/files/components/tesseract-3.02/{0}/download";

        private readonly OcrDependencyManager ocrDependencyManager;

        public FOcrLanguageDownload(OcrDependencyManager ocrDependencyManager)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            InitializeComponent();

            // Add missing languages to the list of language options
            // Special case for English: sorted first, and checked by default
            var languageOptions = this.ocrDependencyManager.GetMissingLanguages().OrderBy(x => x.Code == "eng" ? "AAA" : x.LangName);
            foreach (var languageOption in languageOptions)
            {
                var item = new ListViewItem { Text = languageOption.LangName, Tag = languageOption };
                if (languageOption.Code == "eng")
                {
                    item.Checked = true;
                }
                lvLanguages.Items.Add(item);
            }

            UpdateView();
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(lvLanguages)
                    .WidthToForm()
                    .HeightToForm()
                .Bind(labelSizeEstimate, btnCancel, btnDownload)
                    .BottomToForm()
                .Bind(btnCancel, btnDownload)
                    .RightToForm()
                .Activate();
        }

        private void UpdateView()
        {
            double downloadSize =
                lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => ((OcrLanguage)x.Tag).Size).Sum();
            if (!ocrDependencyManager.IsExecutableDownloaded)
            {
                downloadSize += ocrDependencyManager.ExecutableFileSize;
            }
            labelSizeEstimate.Text = string.Format(MiscResources.EstimatedDownloadSize, downloadSize.ToString("f1"));

            btnDownload.Enabled = lvLanguages.Items.Cast<ListViewItem>().Any(x => x.Checked);
        }

        private void lvLanguages_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateView();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            var progressForm = FormFactory.Create<FDownloadProgress>();
            if (!ocrDependencyManager.IsExecutableDownloaded)
            {
                progressForm.QueueFile(DownloadBase, ocrDependencyManager.ExecutableFileName, ocrDependencyManager.ExecutableFileSha1, tempPath =>
                {
                    string exeFilePath = Path.Combine(ocrDependencyManager.GetExecutableDir().FullName,
                        ocrDependencyManager.ExecutableFileName.Replace(".gz", ""));
                    DecompressFile(tempPath, exeFilePath);
                });
            }
            foreach (
                var lang in
                    lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => (OcrLanguage)x.Tag))
            {
                progressForm.QueueFile(DownloadBase, lang.Filename, lang.Sha1, tempPath =>
                {
                    string langFilePath = Path.Combine(ocrDependencyManager.GetLanguageDir().FullName,
                        lang.Filename.Replace(".gz", ""));
                    DecompressFile(tempPath, langFilePath);
                });
            }
            Close();
            progressForm.ShowDialog();
        }

        private static void DecompressFile(string sourcePath, string destPath)
        {
            using (FileStream inFile = new FileInfo(sourcePath).OpenRead())
            {
                using (FileStream outFile = File.Create(destPath))
                {
                    using (GZipStream decompress = new GZipStream(inFile, CompressionMode.Decompress))
                    {
                        decompress.CopyTo(outFile);
                    }
                }
            }
        }
    }
}
