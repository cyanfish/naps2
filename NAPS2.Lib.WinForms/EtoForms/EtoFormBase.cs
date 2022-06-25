using Eto.Forms;
using NAPS2.WinForms;

namespace NAPS2.EtoForms;

public abstract class EtoFormBase : Form, IFormBase
{
    protected EtoFormBase(Naps2Config config)
    {
        Config = config;
        FormStateController = new FormStateController(this, config);
    }

    public IFormStateController FormStateController { get; }

    public IFormFactory FormFactory { get; set; }
        
    public Naps2Config Config { get; set; }
}