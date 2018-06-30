using NAPS2.Lang.Resources;
using NAPS2.Scan;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

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
                .Bind(ComboName)
                    .WidthToForm()
                .Bind(textboxWidth, textboxHeight)
                    .WidthTo(() => Width / 3)
                .Bind(comboUnit)
                    .WidthTo(() => Width - (2 * (Width / 3)))
                .Bind(LabelX)
                    .LeftTo(() => textboxWidth.Right)
                .Bind(textboxHeight)
                    .LeftTo(() => LabelX.Right)
                .Bind(comboUnit)
                    .LeftTo(() => textboxHeight.Right)
                .Bind(BtnCancel, BtnOK, BtnDelete)
                    .RightToForm()
                .Activate();

            initialDimens = PageSizeDimens ?? ScanPageSize.Letter.PageDimensions();

            UpdateDropdown();
            ComboName.Text = PageSizeName ?? "";
            UpdateDimens(initialDimens);
        }

        private void UpdateDropdown()
        {
            ComboName.Items.Clear();
            foreach (var preset in UserConfigManager.Config.CustomPageSizePresets.OrderBy(x => x.Name))
            {
                ComboName.Items.Add(preset.Name);
            }
        }

        private void UpdateDimens(PageDimensions dimens)
        {
            textboxWidth.Text = dimens.Width.ToString(CultureInfo.CurrentCulture);
            textboxHeight.Text = dimens.Height.ToString(CultureInfo.CurrentCulture);
            comboUnit.SelectedIndex = (int)dimens.Unit;
        }

        private void ComboName_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var presets = UserConfigManager.Config.CustomPageSizePresets;
            var dimens = presets.Where(x => x.Name == (string)ComboName.SelectedItem).Select(x => x.Dimens).FirstOrDefault();
            if (dimens != null)
            {
                UpdateDimens(dimens);
            }
        }

        private void ComboName_TextChanged(object sender, EventArgs e)
        {
            var presets = UserConfigManager.Config.CustomPageSizePresets;
            BtnDelete.Enabled = presets.Any(x => x.Name == ComboName.Text);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnOK_Click(object sender, EventArgs e)
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
            if (!string.IsNullOrWhiteSpace(ComboName.Text))
            {
                PageSizeName = ComboName.Text;
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

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(string.Format(MiscResources.ConfirmDelete, ComboName.Text), MiscResources.Delete, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                var presets = UserConfigManager.Config.CustomPageSizePresets;
                presets.RemoveAll(x => x.Name == ComboName.Text);
                UserConfigManager.Save();

                UpdateDropdown();
                ComboName.Text = "";
                UpdateDimens(initialDimens);
            }
        }
    }
}