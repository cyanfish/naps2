using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

/// <summary>
/// A set of settings controls that can be hosted by a settings form.
/// </summary>
internal abstract class SettingsPanel
{
    protected SettingsPanel(IFormBase form)
    {
        Form = form;
    }

    protected IFormBase Form { get; }

    protected Naps2Config Config => Form.Config;

    public abstract string Title { get; }

    public abstract string IconName { get; }

    public virtual LayoutElement? BuildScrollableLayout() => null;

    public virtual LayoutElement? BuildFixedLayout() => null;

    public abstract void Save();

    public abstract void RestoreDefaults();

    public virtual void Cancel()
    {
    }

    public virtual void OnShown()
    {
    }
}