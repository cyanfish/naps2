using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Scan;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public class FormBase : Form, IInvoker
    {
        private bool loaded;

        public FormBase()
        {
            UpdateRTL();

            RestoreFormState = true;
            SaveFormState = true;

            Load += OnLoadInternal;
            Closed += OnClosed;
            Resize += OnResize;
            Move += OnMove;
        }

        public IFormFactory FormFactory { get; set; }

        public IUserConfigManager UserConfigManager { get; set; }

        protected bool RestoreFormState { get; set; }

        protected bool SaveFormState { get; set; }

        #region Helper Properties

        private List<FormState> FormStates
        {
            get
            {
                if (UserConfigManager == null)
                {
                    // Should only occur with the designer
                    return new List<FormState>();
                }
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

        #region Helper Methods

        protected void AddEnumItems<T>(ComboBox combo)
        {
            AddEnumItems<T>(combo, Combo_Format);
        }

        protected void AddEnumItems<T>(ComboBox combo, ListControlConvertEventHandler format)
        {
            foreach (object item in Enum.GetValues(typeof(T)))
            {
                combo.Items.Add(item);
            }
            combo.Format += format;
        }

        void Combo_Format(object sender, ListControlConvertEventArgs e)
        {
            e.Value = ((Enum)e.ListItem).Description();
        }

        public void Invoke(Action action)
        {
            ((Control) this).Invoke(action);
        }

        public T InvokeGet<T>(Func<T> func)
        {
            T value = default;
            Invoke(() => value = func());
            return value;
        }

        public void SafeInvoke(Action action)
        {
            try
            {
                Invoke(action);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void SafeInvokeAsync(Action action)
        {
            try
            {
                BeginInvoke(action);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        #endregion

        protected void UpdateRTL()
        {
            bool isRTL = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
            RightToLeft = isRTL ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = isRTL;
        }

        /// <summary>
        /// Descendant forms should override this instead of subscribing to the Load event when logic needs
        /// to be performed before the form is resized (e.g. setting up LayoutManager).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected virtual void OnLoad(object sender, EventArgs eventArgs)
        {
        }

        #region Event Handlers

        private void OnLoadInternal(object sender, EventArgs eventArgs)
        {
            OnLoad(sender, eventArgs);

            if (FormState != null && RestoreFormState)
            {
                DoRestoreFormState();
            }
            loaded = true;
        }

        protected void DoRestoreFormState()
        {
            FormState formState = FormState;
            if (!formState.Location.IsEmpty)
            {
                if (Screen.AllScreens.Any(x => x.WorkingArea.Contains(formState.Location)))
                {
                    // Only move to the specified location if it's onscreen
                    // It might be offscreen if the user has disconnected a monitor
                    Location = formState.Location;
                }
            }
            if (!formState.Size.IsEmpty)
            {
                Size = formState.Size;
            }
            if (formState.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            if (loaded && SaveFormState)
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
            if (loaded && SaveFormState)
            {
                if (WindowState == FormWindowState.Normal)
                {
                    FormState.Location = Location;
                }
            }
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            if (SaveFormState)
            {
                UserConfigManager?.Save();
            }
        }

        #endregion
    }
}
