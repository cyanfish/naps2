using NAPS2.EntryPoints;

namespace NAPS2;

static class Program
{
    /// <summary>
    /// The NAPS2.app main method.
    /// </summary>
    static void Main(string[] args)
    {
        // Use reflection to avoid antivirus false positives (yes, really)
        typeof(GtkEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
    }
}