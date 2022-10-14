using Eto.Drawing;

namespace NAPS2.EtoForms;

public interface IFormStateController
{
    bool SaveFormState { get; set; }
    bool RestoreFormState { get; set; }
    Size MinimumClientSize { get; set; }
    Size DefaultClientSize { get; set; }
    string FormName { get; }
}