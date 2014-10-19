using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Ocr;

namespace NAPS2.WinForms
{
    public partial class FOcrLanguageDownload : FormBase
    {
        private readonly OcrLanguageManager _ocrLanguageManager;

        public FOcrLanguageDownload(OcrLanguageManager ocrLanguageManager)
        {
            _ocrLanguageManager = ocrLanguageManager;
            InitializeComponent();

            // Add missing languages to the list of language options
            // Special case for English: sorted first, and checked by default
            var languageOptions = _ocrLanguageManager.GetMissingLanguages().OrderBy(x => x.Code == "eng" ? "aaa" : x.Code);
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
            double langDownloadSize =
                lvLanguages.Items.Cast<ListViewItem>().Where(x => x.Checked).Select(x => ((OcrLanguage)x.Tag).Size).Sum();
            labelSizeEstimate.Text = string.Format(MiscResources.EstimatedDownloadSize, langDownloadSize.ToString("f1"));

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
    }
}
