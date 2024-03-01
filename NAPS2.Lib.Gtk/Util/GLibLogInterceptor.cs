using System.Globalization;
using System.Runtime.InteropServices;
using GLib;
using Log = NAPS2.Logging.Log;

namespace NAPS2.Util;

public static class GLibLogInterceptor
{
    /// <summary>
    /// Intercepts GLib/Gtk logging and writes it to the NAPS2 debuglog instead of stdout.
    /// </summary>
    public static void WriteToDebugLog()
    {
        g_log_set_writer_func(Write, IntPtr.Zero, IntPtr.Zero);
    }

    private static void Write(LogLevelFlags flags, IntPtr fields, nint nFields, IntPtr userData)
    {
        string glibDomain = "?";
        string message = "?";
        string priority = "?";

        var size = Marshal.SizeOf<GLogField>();
        for (int i = 0; i < nFields; i++)
        {
            var field = Marshal.PtrToStructure<GLogField>(fields + i * size);
            if (field.key == "PRIORITY")
            {
                var valueStr = Marshal.PtrToStringUTF8(field.value)!;
                if (int.TryParse(valueStr, CultureInfo.InvariantCulture, out var parsedPriority))
                {
                    if (parsedPriority is < 0 or > 4)
                    {
                        // Only log warning or worse
                        return;
                    }
                    priority = parsedPriority switch
                    {
                        0 => "EMERG",
                        1 => "ALERT",
                        2 => "CRITICAL",
                        3 => "ERROR",
                        4 => "WARNING",
                        _ => throw new InvalidOperationException()
                    };
                }
            }
            if (field.key == "GLIB_DOMAIN")
            {
                var valueStr = Marshal.PtrToStringUTF8(field.value)!;
                glibDomain = valueStr;
            }
            if (field.key == "MESSAGE")
            {
                var valueStr = Marshal.PtrToStringUTF8(field.value)!;
                message = valueStr;
            }
        }
        
        Log.Debug($"{glibDomain}-{priority}: {message}");
    }

    [DllImport("libglib-2.0.so.0")]
    private static extern uint g_log_set_writer_func(
        GLogWriterFunc log_func,
        IntPtr user_data,
        IntPtr user_data_free);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GLogWriterFunc(
        LogLevelFlags flags,
        IntPtr fields,
        nint nFields,
        IntPtr user_data);

    [StructLayout(LayoutKind.Sequential)]
    struct GLogField
    {
        public string key;
        public IntPtr value;
        public nint length;
    }
}