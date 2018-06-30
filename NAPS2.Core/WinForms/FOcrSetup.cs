using NAPS2.Config;
using NAPS2.Ocr;
using System;
using System.Linq;
using System.Windows.Forms;

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
                .Bind(comboLanguages, BtnCancel, BtnOK)
                    .RightToForm()
                .Bind(comboLanguages)
                    .WidthToForm()
                .Activate();

            LoadLanguages();
            comboLanguages.DisplayMember = "Name";
            comboLanguages.ValueMember = "Code";

            if (appConfigManager.Config.OcrState == OcrState.Enabled)
            {
                CheckBoxEnableOcr.Checked = true;
                comboLanguages.SelectedValue = (appConfigManager.Config.OcrDefaultLanguage ?? "");
            }
            else if (appConfigManager.Config.OcrState == OcrState.Disabled)
            {
                CheckBoxEnableOcr.Checked = false;
                comboLanguages.SelectedValue = "";
            }
            else
            {
                CheckBoxEnableOcr.Checked = UserConfigManager.Config.EnableOcr;
                comboLanguages.SelectedValue = (UserConfigManager.Config.OcrLanguageCode ?? appConfigManager.Config.OcrDefaultLanguage ?? "");
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
                                     || (appConfigManager.Config.OcrState == OcrState.Enabled
                                        && string.IsNullOrWhiteSpace(appConfigManager.Config.OcrDefaultLanguage));
            CheckBoxEnableOcr.Enabled = canChangeEnabled;
            comboLanguages.Enabled = CheckBoxEnableOcr.Checked && canChangeLanguage;
            LinkGetLanguages.Enabled = canChangeLanguage;
            Label1.Enabled = canChangeLanguage;
            BtnOK.Enabled = canChangeEnabled || canChangeLanguage;
        }

        private void CheckBoxEnableOcr_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void LinkGetLanguages_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormFactory.Create<FOcrLanguageDownload>().ShowDialog();
            var selectedLang = comboLanguages.SelectedItem;
            LoadLanguages();
            comboLanguages.SelectedItem = selectedLang;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            UserConfigManager.Config.EnableOcr = CheckBoxEnableOcr.Checked;
            UserConfigManager.Config.OcrLanguageCode = (string)comboLanguages.SelectedValue;
            UserConfigManager.Save();
            Close();
        }
    }
}