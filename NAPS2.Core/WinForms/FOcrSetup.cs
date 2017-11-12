using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Ocr;

namespace NAPS2.WinForms
{
    public partial class FOcrSetup : FormBase
    {
        private readonly OcrDependencyManager ocrDependencyManager;
        private readonly AppConfigManager appConfigManager;

        public FOcrSetup(OcrDependencyManager ocrDependencyManager, AppConfigManager appConfigManager)
        {
            this.ocrDependencyManager = ocrDependencyManager;
            this.appConfigManager = appConfigManager;
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

            if (appConfigManager.Config.OcrState == OcrState.Enabled)
            {
                checkBoxEnableOcr.Checked = true;
                comboLanguages.SelectedValue = appConfigManager.Config.OcrDefaultLanguage ?? "";
                if (comboLanguages.SelectedValue == null)
                {
                    comboLanguages.SelectedValue = comboLanguages.Items.Cast<Language>().Select(x => x.Code).FirstOrDefault() ?? "";
                }
            }
            else if (appConfigManager.Config.OcrState == OcrState.Disabled)
            {
                checkBoxEnableOcr.Checked = false;
                comboLanguages.SelectedValue = "";
            }
            else
            {
                checkBoxEnableOcr.Checked = UserConfigManager.Config.EnableOcr;
                comboLanguages.SelectedValue = UserConfigManager.Config.OcrLanguageCode ?? appConfigManager.Config.OcrDefaultLanguage ?? "";
                if (comboLanguages.SelectedValue == null)
                {
                    comboLanguages.SelectedValue = comboLanguages.Items.Cast<Language>().Select(x => x.Code).FirstOrDefault() ?? "";
                }
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
            bool canChangeEnabled = appConfigManager.Config.OcrState == OcrState.UserConfig;
            bool canChangeLanguage = appConfigManager.Config.OcrState == OcrState.UserConfig
                                     || appConfigManager.Config.OcrState == OcrState.Enabled
                                        && string.IsNullOrWhiteSpace(appConfigManager.Config.OcrDefaultLanguage);
            checkBoxEnableOcr.Enabled = canChangeEnabled;
            comboLanguages.Enabled = checkBoxEnableOcr.Checked && canChangeLanguage;
            linkGetLanguages.Enabled = canChangeLanguage;
            label1.Enabled = canChangeLanguage;
            btnOK.Enabled = canChangeEnabled || canChangeLanguage;
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
