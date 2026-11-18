#if TEST_SCENARIOS
using System.Runtime.InteropServices;
using System.Text;
using Eto.Drawing;
using NAPS2.Images.Bitwise;
using NAPS2.Platform.Windows;

namespace NAPS2.TestScenarios;

/// <summary>
/// Window access for Windows (WinForms and WPF), using PrintWindow so captures work even when the window is covered or
/// not in the foreground.
/// </summary>
internal class Win32TestScenarioPlatform(ImageContext imageContext) : ITestScenarioPlatform
{
    private const uint PW_RENDERFULLCONTENT = 2;
    private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
    private const int DWMWA_CLOAKED = 14;

    public List<TestScenarioWindow> GetWindows()
    {
        var pid = (uint) Environment.ProcessId;
        var windows = new List<TestScenarioWindow>();
        // EnumWindows enumerates top-level windows in z-order (front to back)
        Win32.EnumWindows((hwnd, _) =>
        {
            Win32.GetWindowThreadProcessId(hwnd, out var windowPid);
            if (windowPid == pid && Win32.IsWindowVisible(hwnd) && !IsCloaked(hwnd))
            {
                var bounds = GetFrameBounds(hwnd);
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    windows.Add(new TestScenarioWindow(GetTitle(hwnd), bounds, hwnd));
                }
            }
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    public unsafe void Capture(TestScenarioWindow window, string path)
    {
        var hwnd = (IntPtr) window.Handle;
        if (!Win32.GetWindowRect(hwnd, out var rect))
        {
            throw new InvalidOperationException("Could not get the window bounds");
        }
        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;
        // The window rect includes invisible resize borders, so we crop to the visible frame
        var frame = GetFrameBounds(hwnd);
        var crop = new Rectangle(frame.X - rect.Left, frame.Y - rect.Top, frame.Width, frame.Height);

        var screenDc = Win32.GetDC(IntPtr.Zero);
        var memDc = Win32.CreateCompatibleDC(screenDc);
        var header = new Win32.BitmapInfoHeader
        {
            biSize = Marshal.SizeOf<Win32.BitmapInfoHeader>(),
            biWidth = width,
            biHeight = -height,
            biPlanes = 1,
            biBitCount = 32
        };
        var hBitmap = Win32.CreateDIBSection(memDc, ref header, 0, out var bits, IntPtr.Zero, 0);
        var oldBitmap = Win32.SelectObject(memDc, hBitmap);
        try
        {
            if (!Win32.PrintWindow(hwnd, memDc, PW_RENDERFULLCONTENT))
            {
                throw new InvalidOperationException("PrintWindow failed");
            }
            Win32.GdiFlush();
            // GDI doesn't reliably write the alpha channel
            var ptr = (byte*) bits;
            for (int i = 3; i < width * height * 4; i += 4)
            {
                ptr[i] = 255;
            }
            using var image = imageContext.Create(crop.Width, crop.Height, ImagePixelFormat.ARGB32);
            new CopyBitwiseImageOp
            {
                SourceXOffset = crop.X,
                SourceYOffset = crop.Y,
                Columns = crop.Width,
                Rows = crop.Height
            }.Perform(bits, new PixelInfo(width, height, SubPixelType.Bgra), image);
            image.Save(path, ImageFileFormat.Png);
        }
        finally
        {
            Win32.SelectObject(memDc, oldBitmap);
            Win32.DeleteObject(hBitmap);
            Win32.DeleteDC(memDc);
            Win32.ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    private static string GetTitle(IntPtr hwnd)
    {
        var sb = new StringBuilder(512);
        Win32.GetWindowText(hwnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static Rectangle GetFrameBounds(IntPtr hwnd)
    {
        if (Win32.DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out Win32.Rect rect,
                Marshal.SizeOf<Win32.Rect>()) != 0)
        {
            Win32.GetWindowRect(hwnd, out rect);
        }
        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public bool IsActive(TestScenarioWindow window) => GetForegroundWindow() == (IntPtr) window.Handle;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private static bool IsCloaked(IntPtr hwnd)
    {
        return Win32.DwmGetWindowAttribute(hwnd, DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0 && cloaked != 0;
    }
}
#endif
