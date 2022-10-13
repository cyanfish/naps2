using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms;

public class FormStateController : IFormStateController
{
    private readonly Window _window;
    private readonly Naps2Config _config;
    private FormState? _formState;
    private bool _loaded;

    public FormStateController(Window window, Naps2Config config)
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

    private void OnLoadInternal(object? sender, EventArgs eventArgs)
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
        var location = new Point(_formState.Location.X, _formState.Location.Y);
        var size = new Size(_formState.Size.Width, _formState.Size.Height);
        if (!location.IsZero)
        {
            if (Screen.Screens.Any(x => x.WorkingArea.Contains(location)))
            {
                // Only move to the specified location if it's onscreen
                // It might be offscreen if the user has disconnected a monitor
                _window.Location = location;
            }
        }
        if (!size.IsEmpty)
        {
            EtoPlatform.Current.SetFormSize(_window, size);
        }
        if (_formState.Maximized)
        {
            _window.WindowState = WindowState.Maximized;
        }
    }

    private void OnResize(object? sender, EventArgs eventArgs)
    {
        if (_loaded && _formState != null && SaveFormState)
        {
            _formState.Maximized = (_window.WindowState == WindowState.Maximized);
            if (_window.WindowState == WindowState.Normal)
            {
                var size = EtoPlatform.Current.GetFormSize(_window);
                _formState.Size = new FormState.FormSize(size.Width, size.Height);
            }
        }
    }

    private void OnMove(object? sender, EventArgs eventArgs)
    {
        if (_loaded && _formState != null && SaveFormState)
        {
            if (_window.WindowState == WindowState.Normal)
            {
                _formState.Location = new FormState.FormLocation(_window.Location.X, _window.Location.Y);
            }
        }
    }

    private void OnClosed(object? sender, EventArgs eventArgs)
    {
        if (SaveFormState && _formState != null)
        {
            var formStates = _config.Get(c => c.FormStates);
            formStates = formStates.RemoveAll(fs => fs.Name == FormName).Add(_formState);
            _config.User.Set(c => c.FormStates, formStates);
        }
    }
}