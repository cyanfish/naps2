using Eto.Forms;
using NAPS2.WinForms;

namespace NAPS2.EtoForms;

public abstract class EtoDialogBase : Dialog, IFormBase
{
    protected EtoDialogBase(ScopedConfig scopedConfig)
    {
        Config = scopedConfig;
        FormStateController = new FormStateController(this, scopedConfig);
    }

    public FormStateController FormStateController { get; }

    public IFormFactory FormFactory { get; set; }
        
    public ScopedConfig Config { get; set; }
}