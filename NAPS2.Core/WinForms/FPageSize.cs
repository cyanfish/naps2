using System;
using System.Collections.Generic;
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
            decimal width, height;
            if (!decimal.TryParse(textboxWidth.Text, out width))
            {
                textboxWidth.Focus();
                return;
            }
            if (!decimal.TryParse(textboxHeight.Text, out height))
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
