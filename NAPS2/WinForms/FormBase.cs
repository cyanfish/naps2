using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Config;
using Ninject;

namespace NAPS2.WinForms
{
    public class FormBase : Form
    {
        public FormBase(IKernel kernel)
        {
            Kernel = kernel;
            Load += OnLoad;
            Closed += OnClosed;
        }

        public IKernel Kernel { get; private set; }

        private void OnLoad(object sender, EventArgs eventArgs)
        {
            var formState = FormState;
            if (formState != null)
            {
                Location = formState.Location;
                Size = formState.Size;
                if (formState.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }
            }
        }

        private UserConfigManager UserConfigManager
        {
            get
            {
                return Kernel.Get<UserConfigManager>();
            }
        }

        private List<KeyValuePair<string, FormState>> FormStates
        {
            get
            {
                return UserConfigManager.Config.FormStates;
            }
        }

        private FormState FormState
        {
            get
            {
                return FormStates.Where(x => x.Key == Name).Select(x => x.Value).SingleOrDefault();
            }
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            var formState = FormState;
            if (formState == null)
            {
                formState = new FormState();
                FormStates.Add(new KeyValuePair<string, FormState>(Name, formState));
            }
            formState.Location = Location;
            formState.Size = Size;
            formState.Maximized = (WindowState == FormWindowState.Maximized);
            UserConfigManager.Save();
        }
    }
}
