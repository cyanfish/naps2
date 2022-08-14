using System.Windows.Forms;
using Xunit;

namespace NAPS2.Lib.Tests.Asserts;

public static class FormAsserts
{
    public static void Visible(Control control)
    {
        Form form = null;
        for (var node = control; node != null; node = node.Parent)
        {
            if (node is Form f)
            {
                form = f;
                break;
            }
            Assert.True(node.Visible);
        }
        Assert.NotNull(form);
    }

    public static void NotVisible(Control control)
    {
        Form form = null;
        bool someInvisible = false;
        for (var node = control; node != null; node = node.Parent)
        {
            if (node is Form f)
            {
                form = f;
                break;
            }
            someInvisible = someInvisible || !node.Visible;
        }
        Assert.True(someInvisible);
        Assert.NotNull(form);
    }
}