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
        private readonly OcrEngineManager ocrEngineManager;
        private readonly List<OcrMode> availableModes;

        public FOcrSetup(OcrEngineManager ocrEngineManager)
        {
            this.ocrEngineManager = ocrEngineManager;
            InitializeComponent();

            comboOcrMode.Format += (sender, e) => e.Value = ((Enum)e.ListItem).Description();
            availableModes = ocrEngineManager.ActiveEngine?.SupportedModes?.ToList();
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

            checkBoxEnableOcr.Checked = ConfigProvider.Get(c => c.EnableOcr);
            SetSelectedValue(comboLanguages, ConfigProvider.Get(c => c.OcrLanguageCode));
            SetSelectedItem(comboOcrMode, ConfigProvider.Get(c => c.OcrMode));
            checkBoxRunInBG.Checked = ConfigProvider.Get(c => c.OcrAfterScanning);

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
            var languages = ocrEngineManager.ActiveEngine?.InstalledLanguages
                .OrderBy(x => x.Name)
                .ToList();
            comboLanguages.DataSource = languages;

            linkGetLanguages.Visible = ocrEngineManager.EngineToInstall != null;
        }

        private void UpdateView()
        {
            bool canChangeEnabled = ConfigScopes.AppLocked.Get(c => c.EnableOcr) == null;
            bool canChangeLanguage = canChangeEnabled
                                     || ConfigProvider.Get(c => c.EnableOcr)
                                        && ConfigScopes.AppLocked.Get(c => c.OcrLanguageCode) == null;
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
            if (ConfigScopes.AppLocked.Get(c => c.EnableOcr) == null)
            {
                ConfigScopes.User.SetAll(new CommonConfig
                {
                    EnableOcr = checkBoxEnableOcr.Checked,
                    OcrLanguageCode = (string)comboLanguages.SelectedValue,
                    OcrMode = availableModes != null ? (OcrMode)comboOcrMode.SelectedItem : OcrMode.Default,
                    OcrAfterScanning = checkBoxRunInBG.Checked
                });
            }
            Close();
        }
    }
}
