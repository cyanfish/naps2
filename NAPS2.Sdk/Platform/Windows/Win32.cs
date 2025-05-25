using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace NAPS2.Platform.Windows;

/// <summary>
/// Helper class for common Win32 methods called via P/Invoke.
/// </summary>
internal static class Win32
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr AddDllDirectory(string directory);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SetDllDirectory(string directory);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetDefaultDllDirectories(int directoryFlags);

    public const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string path);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr module, string procName);

    [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
    public static extern void CopyMemory(IntPtr dst, IntPtr src, uint len);

    [DllImport("user32.dll")]
    public static extern int GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(in Message lpmsg);

    [DllImport("user32.dll")]
    public static extern bool DispatchMessage(ref Message lpmsg);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string? lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    public static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern UInt16 RegisterClassW(in WndClass lpWndClass);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint RegisterWindowMessage(string lpString);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

    public enum ShowWindowCommands
    {
        Hide = 0,
        Normal = 1,
        ShowMinimized = 2,
        Maximize = 3,
        ShowMaximized = 3,
        ShowNoActivate = 4,
        Show = 5,
        Minimize = 6,
        ShowMinNoActive = 7,
        ShowNA = 8,
        Restore = 9,
        ShowDefault = 10,
        ForceMinimize = 11
    }

    public struct Message
    {
        public IntPtr hWnd;
        public int msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point pt;
    }

    public struct Point
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WndClass
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
    }

    public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    public class ShellLink;

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    public interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
    
    [Flags]
    public enum ASSOC_FILTER
    {
        ASSOC_FILTER_NONE = 0x00000000,
        ASSOC_FILTER_RECOMMENDED = 0x00000001
    }

    [Guid("973810ae-9599-4b88-9e4d-6ee98c9552da"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumAssocHandlers
    {
        [PreserveSig]
        int Next(int celt, out IAssocHandler rgelt, out int pceltFetched);
    }

    [Guid("f04061ac-1659-4a3f-a954-775aa57fc083"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAssocHandler
    {
        void GetName([MarshalAs(UnmanagedType.LPWStr)] out string ppsz);
        void GetUIName([MarshalAs(UnmanagedType.LPWStr)] out string ppsz);
        void GetIconLocation([MarshalAs(UnmanagedType.LPWStr)] out string ppszPath, out int pIndex);
        [PreserveSig]
        int IsRecommended();
        void MakeDefault([MarshalAs(UnmanagedType.LPWStr)] string pszDescription);
        void Invoke(IDataObject pdo);
        void CreateInvoker(IDataObject pdo, out /*IAssocHandlerInvoker*/ object invoker);
    }

    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IShellItem
    {
        IDataObject BindToHandler(IBindCtx? pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
    }
    
    [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
    public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    public extern static int SHCreateItemFromParsingName(string pszPath, IBindCtx? pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    public extern static int SHAssocEnumHandlers(string pszExtra, ASSOC_FILTER afFilter, out IEnumAssocHandlers ppEnumHandler);
}