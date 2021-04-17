using NAPS2.Config;
using NAPS2.WinForms;

namespace NAPS2.EtoForms
{
    public interface IFormBase
    {
        // TODO: Remove these, Eto requires things to be defined in the constructor so property injection is error-prone
        IFormFactory FormFactory { get; set; }

        ConfigScopes ConfigScopes { get; set; }

        ScopeSetConfigProvider<CommonConfig> ConfigProvider { get; set; }
    }
}