namespace NAPS2.Util;

public class MacProcessHelper
{
    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int TransformProcessType(ref ProcessSerialNumber psn, int type);

    private static ProcessSerialNumber _currentProcess = new() { hi = 0, lo = 2 };

    public static void TransformThisProcessToBackground()
    {
        TransformProcessType(ref _currentProcess, 2);
    }

    public static void TransformThisProcessToForeground()
    {
        TransformProcessType(ref _currentProcess, 1);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessSerialNumber
    {
        public int hi;
        public int lo;
    }
}