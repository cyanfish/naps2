namespace NAPS2.EtoForms;

/// <summary>
/// Where an icon is shown, which determines its size (and for some platforms, which icon is used).
/// </summary>
public enum IconScenario
{
    Menu,
    Button,
    LargeButton,
    Toolbar,
    SmallToolbar,
    Inline16,
    Inline32,
    Inline48
}

public static class IconScenarioExtensions
{
    public static int ToSize(this IconScenario scenario) => scenario switch
    {
        IconScenario.Menu or IconScenario.Button or IconScenario.SmallToolbar or IconScenario.Inline16 => 16,
        IconScenario.LargeButton or IconScenario.Toolbar or IconScenario.Inline32 => 32,
        IconScenario.Inline48 => 48,
        _ => throw new ArgumentException()
    };
}