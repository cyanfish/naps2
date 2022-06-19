using System.Windows.Forms;
using NAPS2.Ocr;

namespace NAPS2.WinForms
{
    public partial class FOcrLanguageDownload : FormBase
    {
        private readonly TesseractLanguageManager _tesseractLanguageManager;

        public FOcrLanguageDownload(TesseractLanguageManager tesseractLanguageManager)
        {
            _tesseractLanguageManager = tesseractLanguageManager;
            InitializeComponent();

            var initialSelection = new HashSet<string>();
            // TODO: We used to select old installed languages here, maybe we could do it again if we get new lang data 
            if (!_tesseractLanguageManager.InstalledLanguages.Any())
            {
                // Fresh install, so pre-select English as a sensible default
                initialSelection.Add("ocr-eng");
            }

            // Populate the list of language options
            // Special case for English: sorted to the top of the list
            var languageOptions = _tesseractLanguageManager.NotInstalledLanguages
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
            double downloadSize = _tesseractLanguageManager.LanguageComponents.Where(x => selectedLanguages.Contains(x.Id)).Select(x => x.DownloadInfo.Size).Sum();

            labelSizeEstimate.Text = string.Format(MiscResources.EstimatedDownloadSize, downloadSize.ToString("f1"));

            btnDownload.Enabled = lvLanguages.Items.Cast<ListViewItem>().Any(x => x.Checked);
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

            var selectedLanguages = SelectedLanguages;
            foreach (var langComponent in _tesseractLanguageManager.LanguageComponents.Where(x => selectedLanguages.Contains(x.Id)))
            {
                progressForm.QueueFile(langComponent);
            }

            Close();
            progressForm.ShowDialog();
        }
    }
}
