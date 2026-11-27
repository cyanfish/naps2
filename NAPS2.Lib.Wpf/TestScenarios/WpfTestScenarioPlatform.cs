#if TEST_SCENARIOS
using Eto.Forms;
using SWC = System.Windows.Controls;

namespace NAPS2.TestScenarios;

internal class WpfTestScenarioPlatform(ImageContext imageContext)
    : Win32TestScenarioPlatform(imageContext), ITestScenarioPlatform
{
    public void OpenDropDown(DropDown dropDown)
    {
        ((SWC.ComboBox) dropDown.ControlObject).IsDropDownOpen = true;
    }
}
#endif
