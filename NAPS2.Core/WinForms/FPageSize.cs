using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Scan;

namespace NAPS2.WinForms
{
    public partial class FPageSize : FormBase
    {
        public FPageSize()
        {
            InitializeComponent();

            textboxWidth.Text = 8.5.ToString(CultureInfo.CurrentCulture);
            textboxHeight.Text = 11.ToString(CultureInfo.CurrentCulture);

            AddEnumItems<PageSizeUnit>(comboUnit);
            comboUnit.SelectedIndex = 0;
        }

        public PageDimensions Result { get; private set; }

        protected override void OnLoad(object sender, EventArgs eventArgs)
        {
            new LayoutManager(this)
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
                .Bind(btnCancel, btnOK)
                    .RightToForm()
                .Activate();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            const NumberStyles numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingSign;
            decimal width, height;
            if (!decimal.TryParse(textboxWidth.Text, numberStyle, CultureInfo.CurrentCulture, out width))
            {
                textboxWidth.Focus();
                return;
            }
            if (!decimal.TryParse(textboxHeight.Text, numberStyle, CultureInfo.CurrentCulture, out height))
            {
                textboxHeight.Focus();
                return;
            }
            Result = new PageDimensions
            {
                Width = width,
                Height = height,
                Unit = (PageSizeUnit)comboUnit.SelectedIndex
            };
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
