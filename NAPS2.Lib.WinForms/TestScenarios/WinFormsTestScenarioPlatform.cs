#if TEST_SCENARIOS
using Eto.Forms;
using WF = System.Windows.Forms;

namespace NAPS2.TestScenarios;

internal class WinFormsTestScenarioPlatform(ImageContext imageContext)
    : Win32TestScenarioPlatform(imageContext), ITestScenarioPlatform
{
    public void OpenDropDown(DropDown dropDown)
    {
        ((WF.ComboBox) dropDown.ControlObject).DroppedDown = true;
    }
}
#endif
