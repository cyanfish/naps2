using NAPS2.WinForms;

namespace NAPS2.EtoForms;

public interface IFormBase
{
    IFormStateController FormStateController { get; }

    // TODO: Make these constructor injected, Eto requires things to be defined in the constructor so property injection is error-prone
    IFormFactory FormFactory { get; set; }

    ScopedConfig Config { get; set; }
}