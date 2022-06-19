using Eto.Forms;
using NAPS2.WinForms;

namespace NAPS2.EtoForms;

public abstract class EtoFormBase : Form, IFormBase
{
    protected EtoFormBase(ScopedConfig scopedConfig)
    {
        Config = scopedConfig;
        FormStateController = new FormStateController(this, scopedConfig);
    }

    public IFormStateController FormStateController { get; }

    public IFormFactory FormFactory { get; set; }
        
    public ScopedConfig Config { get; set; }
}