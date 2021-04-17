using Eto.Forms;
using NAPS2.Config;
using NAPS2.WinForms;

namespace NAPS2.EtoForms
{
    public abstract class EtoDialogBase : Dialog, IFormBase
    {
        protected EtoDialogBase(ConfigScopes configScopes)
        {
            ConfigScopes = configScopes;
            ConfigProvider = configScopes.Provider;
            FormStateController = new FormStateController(this, configScopes);
        }

        public FormStateController FormStateController { get; }

        public IFormFactory FormFactory { get; set; }
        
        public ConfigScopes ConfigScopes { get; set; }
        
        public ScopeSetConfigProvider<CommonConfig> ConfigProvider { get; set; }
    }
}