using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NAPS2.Lang.Resources;

namespace NAPS2.WinForms
{
    partial class FAbout : FormBase
    {
        public FAbout()
        {
            RestoreFormState = false;
            InitializeComponent();
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = String.Format(MiscResources.Version, AssemblyVersion);

            // Some of the localization tools I use don't handle line breaks consistently.
            // This compensates by replacing "\n" with actual line breaks. --Ben
            labelCopyright.Text = labelCopyright.Text.Replace("\\n", "\n");
            // Grow the form to fit the copyright text if necessary
            Width = Math.Max(Width, labelCopyright.Right + 25);
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

        #endregion

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel2.Text);
        }
    }
}
