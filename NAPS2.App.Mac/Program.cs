using NAPS2.EntryPoints;

namespace NAPS2;

static class Program
{
    /// <summary>
    /// The NAPS2.app main method.
    /// </summary>
    static void Main(string[] args)
    {
        // TODO: Clean up exception handling
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Console.WriteLine("In UnhandledException Handler");
        };

        AppDomain.CurrentDomain.FirstChanceException += (sender, args) =>
        {
            Console.WriteLine("In FirstChanceException Handler " + args.Exception);
        };

        ObjCRuntime.Runtime.MarshalManagedException += (sender, args) =>
        {
            Console.WriteLine("In MarshalManagedException Handler");

            args.ExceptionMode = ObjCRuntime.MarshalManagedExceptionMode.UnwindNativeCode;
        };

        ObjCRuntime.Runtime.MarshalObjectiveCException += (sender, args) =>
        {
            Console.WriteLine("In MarshalObjectiveCException Handler");
        };

        // NSUserDefaults.StandardUserDefaults.RegisterDefaults(
        //     NSDictionary.FromObjectAndKey(new NSString("NSApplicationCrashOnExceptions"), new NSNumber(true)));
        // Use reflection to avoid antivirus false positives (yes, really)
        typeof(MacEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
    }
}