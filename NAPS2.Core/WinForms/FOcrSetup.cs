using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Ocr;

namespace NAPS2.WinForms
{
    public partial class FOcrSetup : FormBase
    {
        private readonly OcrDependencyManager ocrDependencyManager;

        public FOcrSetup(OcrDependencyManager ocrDependencyManager)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            InitializeComponent();
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(comboLanguages, btnCancel, btnOK)
                    .RightToForm()
                .Bind(comboLanguages)
                    .WidthToForm()
                .Activate();

            LoadLanguages();
            comboLanguages.DisplayMember = "Name";
            comboLanguages.ValueMember = "Code";

            checkBoxEnableOcr.Checked = UserConfigManager.Config.EnableOcr;
            comboLanguages.SelectedValue = UserConfigManager.Config.OcrLanguageCode;
            if (comboLanguages.SelectedValue == null)
            {
                comboLanguages.SelectedValue = comboLanguages.Items.Cast<string>().FirstOrDefault();
            }

            UpdateView();
        }

        private void LoadLanguages()
        {
            var languages = ocrDependencyManager.InstalledTesseractLanguages
                .OrderBy(x => x.Name)
                .ToList();
            comboLanguages.DataSource = languages;
        }

        private void UpdateView()
        {
            comboLanguages.Enabled = checkBoxEnableOcr.Checked;
        }

        private void checkBoxEnableOcr_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void linkGetLanguages_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
            var selectedLang = comboLanguages.SelectedItem;
            LoadLanguages();
            comboLanguages.SelectedItem = selectedLang;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            UserConfigManager.Config.EnableOcr = checkBoxEnableOcr.Checked;
            UserConfigManager.Config.OcrLanguageCode = (string)comboLanguages.SelectedValue;
            UserConfigManager.Save();
            Close();
        }
    }
}
