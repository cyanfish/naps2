using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;

namespace NAPS2.WinForms
{
    public partial class FOcrLanguageDownload : FormBase
    {
        private readonly OcrDependencyManager ocrDependencyManager;

        public FOcrLanguageDownload(OcrDependencyManager ocrDependencyManager)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            InitializeComponent();

            var initialSelection = new HashSet<string>();
            if (!ocrDependencyManager.HasNewTesseractExe)
            {
                // The new OCR version hasn't been installed yet, so pre-select some languages
                if (ocrDependencyManager.Components.Tesseract302.IsInstalled)
                {
                    // Upgrading from an old version, so pre-select previously used languages
                    foreach (var lang in ocrDependencyManager.Components.Tesseract302Languages.Where(x => x.Value.IsInstalled))
                    {
                        initialSelection.Add(lang.Key);
                    }
                }
                else
                {
                    // Fresh install, so pre-select English as a sensible default
                    initialSelection.Add("eng");
                }
            }

            // Populate the list of language options
            // Special case for English: sorted to the top of the list
            var languageOptions = ocrDependencyManager.Components.Tesseract304Languages.Where(x => !x.Value.IsInstalled)
                .Select(x => ocrDependencyManager.Languages[x.Key])
                .OrderBy(x => x.Code == "eng" ? "AAA" : x.Name);
            foreach (var languageOption in languageOptions)
            {
                var item = new ListViewItem { Text = languageOption.Name, Tag = languageOption.Code };
                if (initialSelection.Contains(languageOption.Code))
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
            var selectedLanguages = lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => ((string)x.Tag));
            double downloadSize = selectedLanguages.Select(x => ocrDependencyManager.Downloads.Tesseract304Languages[x].Size).Sum();

            if (!ocrDependencyManager.Components.Tesseract304.IsInstalled && ocrDependencyManager.Components.Tesseract304.IsSupported)
            {
                downloadSize += ocrDependencyManager.Downloads.Tesseract304.Size;
            }
            if (!ocrDependencyManager.Components.Tesseract304Xp.IsInstalled && !ocrDependencyManager.Components.Tesseract304.IsSupported)
            {
                downloadSize += ocrDependencyManager.Downloads.Tesseract304Xp.Size;
            }

            labelSizeEstimate.Text = string.Format(MiscResources.EstimatedDownloadSize, downloadSize.ToString("f1"));

            btnDownload.Enabled = lvLanguages.Items.Cast<ListViewItem>().Any(x => x.Checked) || ocrDependencyManager.TesseractExeRequiresFix;
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

            if (!ocrDependencyManager.Components.Tesseract304.IsInstalled && ocrDependencyManager.Components.Tesseract304.IsSupported)
            {
                progressForm.QueueFile(ocrDependencyManager.Downloads.Tesseract304,
                    path => ocrDependencyManager.Components.Tesseract304.Install(path));
            }
            if (!ocrDependencyManager.Components.Tesseract304Xp.IsInstalled && !ocrDependencyManager.Components.Tesseract304.IsSupported)
            {
                progressForm.QueueFile(ocrDependencyManager.Downloads.Tesseract304Xp,
                    path => ocrDependencyManager.Components.Tesseract304Xp.Install(path));
            }

            foreach (
                var langCode in
                    lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => (string)x.Tag))
            {
                progressForm.QueueFile(ocrDependencyManager.Downloads.Tesseract304Languages[langCode],
                    path => ocrDependencyManager.Components.Tesseract304Languages[langCode].Install(path));
            }

            Close();
            progressForm.ShowDialog();
        }
    }
}
