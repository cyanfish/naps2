using NAPS2.Config;
using NAPS2.Lang.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace NAPS2.WinForms
{
    internal partial class FAbout : FormBase
    {
        public FAbout(AppConfigManager appConfigManager)
        {
            RestoreFormState = false;
            InitializeComponent();
            LabelProductName.Text = AssemblyProduct;
            LabelVersion.Text = String.Format(MiscResources.Version, AssemblyVersion);

            // Some of the localization tools I use don't handle line breaks consistently.
            // This compensates by replacing "\n" with actual line breaks. --Ben
            LabelCopyright.Text = LabelCopyright.Text.Replace("\\n", "\n");
            // Grow the form to fit the copyright text if necessary
            Width = Math.Max(Width, LabelCopyright.Right + 25);

            BtnDonate.Visible &= !appConfigManager.Config.HideDonateButton;
        }

        #region Assembly Attribute Accessors

        private static string GetAssemblyAttributeValue<T>(Func<T, string> selector)
        {
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), false);
            if (attributes.Length == 0)
            {
                return "";
            }
            return selector((T)attributes[0]);
        }

        public string AssemblyTitle
        {
            get
            {
                string title = GetAssemblyAttributeValue<AssemblyTitleAttribute>(x => x.Title);
                if (string.IsNullOrEmpty(title))
                {
                    title = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
                }
                return title;
            }
        }

        public string AssemblyVersion => Assembly.GetEntryAssembly().GetName().Version.ToString();

        public string AssemblyDescription => GetAssemblyAttributeValue<AssemblyDescriptionAttribute>(x => x.Description);

        public string AssemblyProduct => GetAssemblyAttributeValue<AssemblyProductAttribute>(x => x.Product);

        public string AssemblyCopyright => GetAssemblyAttributeValue<AssemblyCopyrightAttribute>(x => x.Copyright);

        public string AssemblyCompany => GetAssemblyAttributeValue<AssemblyCompanyAttribute>(x => x.Company);

        #endregion Assembly Attribute Accessors

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(LinkLabel1.Text);
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(LinkLabel2.Text);
        }

        private void BtnDonate_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.naps2.com/donate");
        }
    }
}