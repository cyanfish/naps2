#if !MAC
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using NTwain;

namespace NAPS2.Scan.Internal.Twain;

// TODO: Consider refactoring this to re-use code from Win32MessagePump
/// <summary>
/// A MessageLoopHook implementation that uses Win32 methods directly, with no dependencies on WinForms or WPF.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal class Win32MessageLoopHook : MessageLoopHook
{
    private readonly ILogger _logger;
    private readonly Queue<Action> _queue = new();
    private bool _stopped;
    private Thread? _messageLoopThread;

    public Win32MessageLoopHook(ILogger logger)
    {
        _logger = logger;
    }

    public override void Invoke(Action action)
    {
        if (Thread.CurrentThread == _messageLoopThread)
        {
            action();
            return;
        }
        var toggle = new ManualResetEvent(false);
        lock (_queue)
        {
            _queue.Enqueue(() =>
            {
                action();
                toggle.Set();
            });
            PostMessage(Handle, 0, IntPtr.Zero, IntPtr.Zero);
        }
        toggle.WaitOne();
    }

    public override void BeginInvoke(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
            PostMessage(Handle, 0, IntPtr.Zero, IntPtr.Zero);
        }
    }

    protected override void Start(IWinMessageFilter filter)
    {
        _messageLoopThread = new Thread(() =>
        {
            try
            {
                Handle = CreateWindowEx(0, "Message", null, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                    IntPtr.Zero);
                while (!_stopped && GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
                {
                    if (!filter.IsTwainMessage(Handle, msg.msg, msg.wParam, msg.lParam))
                    {
                        DispatchMessage(ref msg);
                    }

                    var actionsToCall = new List<Action>();
                    lock (_queue)
                    {
                        while (_queue.Count > 0)
                        {
                            actionsToCall.Add(_queue.Dequeue());
                        }
                    }
                    // Run the actions outside the lock to avoid deadlock scenarios
                    foreach (var action in actionsToCall)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in TWAIN message handler");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TWAIN message loop");
            }
        });
        _messageLoopThread.IsBackground = true;
        _messageLoopThread.SetApartmentState(ApartmentState.STA);
        _messageLoopThread.Start();
    }

    protected override void Stop()
    {
        BeginInvoke(() =>
        {
            DestroyWindow(Handle);
            _stopped = true;
        });
    }

    private struct Message
    {
        public IntPtr hWnd;
        public int msg;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point pt;
    }

    private struct Point
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll")]
    private static extern int GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    private static extern bool DispatchMessage(ref Message lpmsg);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string? lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);
}
#endif