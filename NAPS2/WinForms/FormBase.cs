using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAPS2.Config;
using Ninject;

namespace NAPS2.WinForms
{
    public class FormBase : Form
    {
        private bool loaded;

        public FormBase()
        {
            UpdateRTL();

            Load += OnLoad;
            Closed += OnClosed;
            Resize += OnResize;
            Move += OnMove;
        }

        [Inject]
        public IKernel Kernel { get; set; }

        [Inject]
        public UserConfigManager UserConfigManager { get; set; }

        #region Helper Properties

        private List<FormState> FormStates
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
                var state = FormStates.SingleOrDefault(x => x.Name == Name);
                if (state == null)
                {
                    state = new FormState { Name = Name };
                    FormStates.Add(state);
                }
                return state;
            }
        }

        #endregion

        protected void UpdateRTL()
        {
            bool isRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
            RightToLeft = isRTL ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = isRTL;
        }

        #region Event Handlers

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
            loaded = true;
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            if (loaded)
            {
                FormState.Maximized = (WindowState == FormWindowState.Maximized);
                if (WindowState == FormWindowState.Normal)
                {
                    FormState.Size = Size;
                }
            }
        }

        private void OnMove(object sender, EventArgs eventArgs)
        {
            if (loaded)
            {
                if (WindowState == FormWindowState.Normal)
                {
                    FormState.Location = Location;
                }
            }
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            UserConfigManager.Save();
        }

        #endregion
    }
}
