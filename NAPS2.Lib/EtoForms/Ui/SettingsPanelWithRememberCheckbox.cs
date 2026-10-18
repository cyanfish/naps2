using System.Linq.Expressions;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

/// <summary>
/// A settings panel for a settings object that only applies to the current session, unless the user checks "Remember
/// these settings".
/// </summary>
internal abstract class SettingsPanelWithRememberCheckbox<TSettings> : SettingsPanel
{
    private readonly CheckBox _rememberSettings = new() { Text = UiStrings.RememberTheseSettings };

    protected SettingsPanelWithRememberCheckbox(IFormBase form) : base(form)
    {
    }

    protected abstract Expression<Func<CommonConfig, TSettings>> SettingsAccessor { get; }

    protected abstract Expression<Func<CommonConfig, bool>> RememberAccessor { get; }

    protected abstract TSettings SettingsValues { get; set; }

    protected void LoadValues(Naps2Config config)
    {
        SettingsValues = config.Get(SettingsAccessor);
        _rememberSettings.Checked = config.Get(RememberAccessor);
    }

    public override LayoutElement BuildFixedLayout()
    {
        return _rememberSettings;
    }

    public override void Save()
    {
        var settings = SettingsValues;
        bool remember = _rememberSettings.IsChecked();
        if (Equals(settings, Config.Get(SettingsAccessor)) && remember == Config.Get(RememberAccessor))
        {
            // Avoid unnecessarily copying values from other scopes (e.g. defaults)
            return;
        }

        var runTransact = Config.Run.BeginTransaction();
        var userTransact = Config.User.BeginTransaction();
        var transactToWrite = remember ? userTransact : runTransact;

        runTransact.Remove(SettingsAccessor);
        userTransact.Remove(SettingsAccessor);
        transactToWrite.Set(SettingsAccessor, settings);
        userTransact.Set(RememberAccessor, remember);

        runTransact.Commit();
        userTransact.Commit();
    }

    public override void RestoreDefaults()
    {
        LoadValues(Config.DefaultsOnly);
    }
}
