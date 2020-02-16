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
        private readonly OcrEngineManager _ocrEngineManager;
        private readonly IOcrEngine _engineToInstall;

        public FOcrLanguageDownload(OcrEngineManager ocrEngineManager)
        {
            _ocrEngineManager = ocrEngineManager;
            _engineToInstall = ocrEngineManager.EngineToInstall;
            InitializeComponent();

            var initialSelection = new HashSet<string>();
            if (ocrEngineManager.InstalledEngine != null && ocrEngineManager.InstalledEngine != _engineToInstall)
            {
                // Upgrading from an old version, so pre-select previously used languages
                foreach (var lang in ocrEngineManager.InstalledEngine.LanguageComponents.Where(x => x.IsInstalled))
                {
                    initialSelection.Add(lang.Id);
                }
            }

            if (!_engineToInstall.InstalledLanguages.Any())
            {
                // Fresh install, so pre-select English as a sensible default
                initialSelection.Add("ocr-eng");
            }

            // Populate the list of language options
            // Special case for English: sorted to the top of the list
            var languageOptions = _engineToInstall.NotInstalledLanguages
                .OrderBy(x => x.Code == "eng" ? "AAA" : x.Name);
            foreach (var languageOption in languageOptions)
            {
                var item = new ListViewItem { Text = languageOption.Name, Tag = languageOption.Code };
                if (initialSelection.Contains($"ocr-{languageOption.Code}"))
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
            var selectedLanguages = SelectedLanguages;
            double downloadSize = _engineToInstall.LanguageComponents.Where(x => selectedLanguages.Contains(x.Id)).Select(x => x.DownloadInfo.Size).Sum();

            if (!_engineToInstall.IsInstalled)
            {
                downloadSize += _engineToInstall.Component.DownloadInfo.Size;
            }

            labelSizeEstimate.Text = string.Format(MiscResources.EstimatedDownloadSize, downloadSize.ToString("f1"));

            btnDownload.Enabled = lvLanguages.Items.Cast<ListViewItem>().Any(x => x.Checked) || _engineToInstall.InstalledLanguages.Any() && !_engineToInstall.IsInstalled;
        }

        private HashSet<string> SelectedLanguages
        {
            get { return new HashSet<string>(lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => $"ocr-{x.Tag}")); }
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

            if (!_engineToInstall.IsInstalled)
            {
                progressForm.QueueFile(_engineToInstall.Component);
            }

            var selectedLanguages = SelectedLanguages;
            foreach (var langComponent in _engineToInstall.LanguageComponents.Where(x => selectedLanguages.Contains(x.Id)))
            {
                progressForm.QueueFile(langComponent);
            }

            Close();
            progressForm.ShowDialog();
        }
    }
}
