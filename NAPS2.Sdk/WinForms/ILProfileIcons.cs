using System.ComponentModel;

namespace NAPS2.WinForms
{
    public partial class ILProfileIcons : Component
    {
        public ILProfileIcons()
        {
            InitializeComponent();
        }

        public ILProfileIcons(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
