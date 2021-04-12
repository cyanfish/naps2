using Eto.Forms;
using NAPS2.Config;
using NAPS2.WinForms;

namespace NAPS2.EtoForms
{
    public class EtoFormBase : Form, IFormBase
    {
        public IFormFactory FormFactory { get; set; }
        
        public ConfigScopes ConfigScopes { get; set; }
        
        public ScopeSetConfigProvider<CommonConfig> ConfigProvider { get; set; }
    }
}