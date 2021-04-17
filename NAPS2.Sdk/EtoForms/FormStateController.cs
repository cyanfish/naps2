using System;
using System.Linq;
using Eto.Forms;
using NAPS2.Config;

namespace NAPS2.EtoForms
{
    public class FormStateController
    {
        private readonly Window _window;
        private readonly ScopedConfig _config;
        private FormState? _formState;
        private bool _loaded;

        public FormStateController(Window window, ScopedConfig config)
        {
            _window = window;
            _config = config;
            window.SizeChanged += OnResize;
            window.LocationChanged += OnMove;
            window.PreLoad += OnLoadInternal;
            window.Closed += OnClosed;
        }

        public bool SaveFormState { get; set; } = true;

        public bool RestoreFormState { get; set; } = true;

        public string FormName => _window.GetType().Name;

        private void OnLoadInternal(object sender, EventArgs eventArgs)
        {
            if (RestoreFormState || SaveFormState)
            {
                var formStates = _config.Get(c => c.FormStates);
                _formState = formStates.SingleOrDefault(x => x.Name == FormName) ?? new FormState {Name = FormName};
            }

            if (RestoreFormState)
            {
                DoRestoreFormState();
            }
            _loaded = true;
        }

        protected void DoRestoreFormState()
        {
            if (_formState == null)
            {
                throw new InvalidOperationException();
            }
            if (!_formState.Location.IsZero)
            {
                if (Screen.Screens.Any(x => x.WorkingArea.Contains(_formState.Location)))
                {
                    // Only move to the specified location if it's onscreen
                    // It might be offscreen if the user has disconnected a monitor
                    _window.Location = _formState.Location;
                }
            }
            if (!_formState.Size.IsEmpty)
            {
                _window.Size = _formState.Size;
            }
            if (_formState.Maximized)
            {
                _window.WindowState = WindowState.Maximized;
            }
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            if (_loaded && _formState != null && SaveFormState)
            {
                _formState.Maximized = (_window.WindowState == WindowState.Maximized);
                if (_window.WindowState == WindowState.Normal)
                {
                    _formState.Size = _window.Size;
                }
            }
        }

        private void OnMove(object sender, EventArgs eventArgs)
        {
            if (_loaded && _formState != null && SaveFormState)
            {
                if (_window.WindowState == WindowState.Normal)
                {
                    _formState.Location = _window.Location;
                }
            }
        }

        private void OnClosed(object sender, EventArgs eventArgs)
        {
            if (SaveFormState && _formState != null)
            {
                var formStates = _config.Get(c => c.FormStates);
                formStates = formStates.RemoveAll(fs => fs.Name == FormName).Add(_formState);
                _config.User.Set(c => c.FormStates = formStates);
            }
        }
    }
}
