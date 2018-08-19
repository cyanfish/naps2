using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Lang.Resources;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FPageSize : FormBase
    {
        private PageDimensions initialDimens;

        public FPageSize()
        {
            InitializeComponent();

            AddEnumItems<PageSizeUnit>(comboUnit);
        }

        public string PageSizeName { get; set; }

        public PageDimensions PageSizeDimens { get; set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
                .Bind(comboName)
                    .WidthToForm()
                .Bind(textboxWidth, textboxHeight)
                    .WidthTo(() => Width / 3)
                .Bind(comboUnit)
                    .WidthTo(() => Width - 2 * (Width / 3))
                .Bind(labelX)
                    .LeftTo(() => textboxWidth.Right)
                .Bind(textboxHeight)
                    .LeftTo(() => labelX.Right)
                .Bind(comboUnit)
                    .LeftTo(() => textboxHeight.Right)
                .Bind(btnCancel, btnOK, btnDelete)
                    .RightToForm()
                .Activate();

            initialDimens = PageSizeDimens ?? ScanPageSize.Letter.PageDimensions();

            UpdateDropdown();
            comboName.Text = PageSizeName ?? "";
            UpdateDimens(initialDimens);
        }

        private void UpdateDropdown()
        {
            comboName.Items.Clear();
            foreach (var preset in UserConfigManager.Config.CustomPageSizePresets.OrderBy(x => x.Name))
            {
                comboName.Items.Add(preset.Name);
            }
        }

        private void UpdateDimens(PageDimensions dimens)
        {
            textboxWidth.Text = dimens.Width.ToString(CultureInfo.CurrentCulture);
            textboxHeight.Text = dimens.Height.ToString(CultureInfo.CurrentCulture);
            comboUnit.SelectedIndex = (int)dimens.Unit;
        }

        private void comboName_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var presets = UserConfigManager.Config.CustomPageSizePresets;
            var dimens = presets.Where(x => x.Name == (string)comboName.SelectedItem).Select(x => x.Dimens).FirstOrDefault();
            if (dimens != null)
            {
                UpdateDimens(dimens);
            }
        }

        private void comboName_TextChanged(object sender, EventArgs e)
        {
            var presets = UserConfigManager.Config.CustomPageSizePresets;
            btnDelete.Enabled = presets.Any(x => x.Name == comboName.Text);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            const NumberStyles numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingSign;
            if (!decimal.TryParse(textboxWidth.Text, numberStyle, CultureInfo.CurrentCulture, out decimal width))
            {
                textboxWidth.Focus();
                return;
            }
            if (!decimal.TryParse(textboxHeight.Text, numberStyle, CultureInfo.CurrentCulture, out decimal height))
            {
                textboxHeight.Focus();
                return;
            }
            PageSizeName = null;
            PageSizeDimens = new PageDimensions
            {
                Width = width,
                Height = height,
                Unit = (PageSizeUnit)comboUnit.SelectedIndex
            };
            if (!string.IsNullOrWhiteSpace(comboName.Text))
            {
                PageSizeName = comboName.Text;
                var presets = UserConfigManager.Config.CustomPageSizePresets;
                presets.RemoveAll(x => x.Name == PageSizeName);
                presets.Add(new NamedPageSize
                {
                    Name = PageSizeName,
                    Dimens = PageSizeDimens
                });
                UserConfigManager.Save();
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmDelete, comboName.Text), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                var presets = UserConfigManager.Config.CustomPageSizePresets;
                presets.RemoveAll(x => x.Name == comboName.Text);
                UserConfigManager.Save();

                UpdateDropdown();
                comboName.Text = "";
                UpdateDimens(initialDimens);
            }
        }
    }
}
