using Eto.Drawing;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms;

public interface IFormStateController
{
    bool SaveFormState { get; set; }
    bool RestoreFormState { get; set; }
    bool AutoLayoutSize { get; set; }
    Size DefaultClientSize { get; set; }
    Size DefaultExtraLayoutSize { get; set; }
    bool FixedHeightLayout { get; set; }
    string FormName { get; }
    void UpdateLayoutSize(LayoutController layoutController);
}