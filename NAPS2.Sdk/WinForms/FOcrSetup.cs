using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Ocr;
using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class FOcrSetup : FormBase
    {
        private readonly OcrManager ocrManager;
        private readonly List<OcrMode> availableModes;

        public FOcrSetup(OcrManager ocrManager)
        {
            this.ocrManager = ocrManager;
            InitializeComponent();

            comboOcrMode.Format += (sender, e) => e.Value = ((Enum)e.ListItem).Description();
            availableModes = ocrManager.ActiveEngine?.SupportedModes?.ToList();
            if (availableModes != null)
            {
                foreach (var mode in availableModes)
                {
                    comboOcrMode.Items.Add(mode);
                }
            }
        }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(btnCancel, btnOK)
                    .RightToForm()
                .Bind(comboLanguages, comboOcrMode)
                    .WidthToForm()
                .Activate();

            LoadLanguages();
            comboLanguages.DisplayMember = "Name";
            comboLanguages.ValueMember = "Code";

            ConditionalControls.UnlockHeight(this);
            ConditionalControls.SetVisible(comboOcrMode, availableModes != null, 8);
            labelOcrMode.Visible = availableModes != null;
            ConditionalControls.LockHeight(this);

            if (AppConfig.Current.OcrState == OcrState.Enabled)
            {
                checkBoxEnableOcr.Checked = true;
                SetSelectedValue(comboLanguages, AppConfig.Current.OcrDefaultLanguage ?? "");
                SetSelectedItem(comboOcrMode, AppConfig.Current.OcrDefaultMode);
                checkBoxRunInBG.Checked = AppConfig.Current.OcrDefaultAfterScanning;
            }
            else if (AppConfig.Current.OcrState == OcrState.Disabled)
            {
                checkBoxEnableOcr.Checked = false;
                comboLanguages.SelectedValue = "";
                comboOcrMode.SelectedValue = "";
                checkBoxRunInBG.Checked = false;
            }
            else
            {
                checkBoxEnableOcr.Checked = UserConfig.Current.EnableOcr;
                SetSelectedValue(comboLanguages, UserConfig.Current.OcrLanguageCode ?? AppConfig.Current.OcrDefaultLanguage ?? "");
                SetSelectedItem(comboOcrMode, UserConfig.Current.OcrMode == OcrMode.Default ? AppConfig.Current.OcrDefaultMode : UserConfig.Current.OcrMode);
                checkBoxRunInBG.Checked = UserConfig.Current.OcrAfterScanning ?? AppConfig.Current.OcrDefaultAfterScanning;
            }

            UpdateView();
        }

        private void SetSelectedValue(ComboBox combo, object value)
        {
            combo.SelectedValue = value;
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (combo.SelectedValue == null && combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void SetSelectedItem(ComboBox combo, object item)
        {
            combo.SelectedItem = item;
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (combo.SelectedItem == null && combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void LoadLanguages()
        {
            var languages = ocrManager.ActiveEngine?.InstalledLanguages
                .OrderBy(x => x.Name)
                .ToList();
            comboLanguages.DataSource = languages;

            linkGetLanguages.Visible = ocrManager.EngineToInstall != null;
        }

        private void UpdateView()
        {
            bool canChangeEnabled = AppConfig.Current.OcrState == OcrState.UserConfig;
            bool canChangeLanguage = AppConfig.Current.OcrState == OcrState.UserConfig
                                     || AppConfig.Current.OcrState == OcrState.Enabled
                                        && string.IsNullOrWhiteSpace(AppConfig.Current.OcrDefaultLanguage);
            checkBoxEnableOcr.Enabled = canChangeEnabled;
            comboLanguages.Enabled = checkBoxEnableOcr.Checked && canChangeLanguage;
            linkGetLanguages.Enabled = canChangeLanguage;
            label1.Enabled = canChangeLanguage;
            btnOK.Enabled = canChangeEnabled || canChangeLanguage;
            comboOcrMode.Enabled = checkBoxEnableOcr.Checked && canChangeEnabled;
            checkBoxRunInBG.Enabled = checkBoxEnableOcr.Checked && canChangeEnabled;
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
            if (AppConfig.Current.OcrState == OcrState.UserConfig)
            {
                UserConfig.Current.EnableOcr = checkBoxEnableOcr.Checked;
                UserConfig.Current.OcrLanguageCode = (string) comboLanguages.SelectedValue;
                UserConfig.Current.OcrMode = availableModes != null ? (OcrMode) comboOcrMode.SelectedItem : OcrMode.Default;
                UserConfig.Current.OcrAfterScanning = checkBoxRunInBG.Checked;
                UserConfig.Manager.Save();
            }
            Close();
        }
    }
}
