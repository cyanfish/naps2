/*
    NAPS2 (Not Another PDF Scanner 2)
    http://sourceforge.net/projects/naps2/
    
    Copyright (C) 2009       Pavel Sorejs
    Copyright (C) 2012       Michael Adams
    Copyright (C) 2013       Peter De Leeuw
    Copyright (C) 2012-2013  Ben Olden-Cooligan

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
*/

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
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false);
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
                    title = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
                }
                return title;
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                return GetAssemblyAttributeValue<AssemblyDescriptionAttribute>(x => x.Description);
            }
        }

        public string AssemblyProduct
        {
            get
            {
                return GetAssemblyAttributeValue<AssemblyProductAttribute>(x => x.Product);
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                return GetAssemblyAttributeValue<AssemblyCopyrightAttribute>(x => x.Copyright);
            }
        }

        public string AssemblyCompany
        {
            get
            {
                return GetAssemblyAttributeValue<AssemblyCompanyAttribute>(x => x.Company);
            }
        }

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
