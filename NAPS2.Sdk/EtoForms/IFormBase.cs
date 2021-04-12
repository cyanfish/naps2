using NAPS2.Config;
using NAPS2.WinForms;

namespace NAPS2.EtoForms
{
    public interface IFormBase
    {
        IFormFactory FormFactory { get; set; }

        ConfigScopes ConfigScopes { get; set; }

        ScopeSetConfigProvider<CommonConfig> ConfigProvider { get; set; }
    }
}