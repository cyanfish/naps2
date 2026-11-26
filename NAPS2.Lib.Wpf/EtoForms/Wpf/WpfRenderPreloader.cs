using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Media;
using NAPS2.Platform.Windows;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Starts up WPF's render system on background threads, so the UI thread doesn't have to wait for it when the main
/// window is created and shown. Most of that time is spent loading and initializing the graphics driver.
/// </summary>
internal static class WpfRenderPreloader
{
    private const uint D3D_SDK_VERSION = 32;
    private const uint D3DADAPTER_DEFAULT = 0;
    private const int D3DDEVTYPE_HAL = 1;
    private const uint D3DCREATE_FPU_PRESERVE = 0x2;
    private const uint D3DCREATE_MULTITHREADED = 0x4;
    private const uint D3DCREATE_HARDWARE_VERTEXPROCESSING = 0x40;
    private const uint D3DCREATE_NOWINDOWCHANGES = 0x800;
    private const uint D3DSWAPEFFECT_DISCARD = 1;
    private const uint D3DPRESENT_INTERVAL_IMMEDIATE = 0x80000000;

    public static void Start()
    {
        var device = Task.Run(CreateD3DDevice);
        var thread = new Thread(() => StartRenderSystem(device));
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
    }

    private static void StartRenderSystem(Task<IntPtr> device)
    {
        try
        {
            // Setting a root visual connects this thread to WPF's render thread, which starts it up.
            using var source = new HwndSource(new HwndSourceParameters("NAPS2 render preloader")
            {
                Width = 1, Height = 1, WindowStyle = 0
            });
            source.RootVisual = new DrawingVisual();
            if (device.Result != IntPtr.Zero)
            {
                Marshal.Release(device.Result);
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error preloading the WPF render system", ex);
        }
    }

    /// <summary>
    /// Creates a Direct3D 9 device (which WPF renders with) to load and initialize the graphics driver. This runs in
    /// parallel with starting up the render thread, which would otherwise do it on first render.
    /// </summary>
    private static IntPtr CreateD3DDevice()
    {
        if (Win32.Direct3DCreate9Ex(D3D_SDK_VERSION, out var d3d) != 0)
        {
            return IntPtr.Zero;
        }
        try
        {
            var parameters = new Win32.D3DPresentParameters
            {
                BackBufferWidth = 1,
                BackBufferHeight = 1,
                BackBufferCount = 1,
                SwapEffect = D3DSWAPEFFECT_DISCARD,
                DeviceWindow = Win32.GetDesktopWindow(),
                Windowed = 1,
                PresentationInterval = D3DPRESENT_INTERVAL_IMMEDIATE
            };
            int hr = d3d.CreateDeviceEx(D3DADAPTER_DEFAULT, D3DDEVTYPE_HAL, parameters.DeviceWindow,
                D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE |
                D3DCREATE_NOWINDOWCHANGES, ref parameters, IntPtr.Zero, out var device);
            return hr == 0 ? device : IntPtr.Zero;
        }
        finally
        {
            Marshal.ReleaseComObject(d3d);
        }
    }
}
