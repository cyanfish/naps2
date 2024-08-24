using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Platform.Windows;

internal class Win32MessagePump : IInvoker, IDisposable
{
    public static Win32MessagePump Create()
    {
        return new Win32MessagePump();
    }

    private readonly Queue<Action> _queue = new();
    private readonly Thread? _messageLoopThread;
    private bool _stopped;

    private Win32MessagePump()
    {
        _messageLoopThread = Thread.CurrentThread;
        Handle = CreateWindowEx(0, "Message", "", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    public void RunMessageLoop()
    {
        try
        {
            while (!_stopped && GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                DispatchMessage(ref msg);

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
                        Logger.LogError(ex, "Error in message handler");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in message loop");
        }
    }

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public IntPtr Handle { get; }

    public void Invoke(Action action)
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

    public void InvokeDispatch(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
            PostMessage(Handle, 0, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public T InvokeGet<T>(Func<T> func)
    {
        T value = default!;
        Invoke(() => value = func());
        return value;
    }

    public IntPtr CreateBackgroundWindow(IntPtr parent = default)
    {
        return InvokeGet(() =>
            CreateWindowEx(0, "Message", "", 0, 0, 0, 0, 0, parent, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
    }

    public void CloseWindow(IntPtr window)
    {
        InvokeDispatch(() => { DestroyWindow(window); });
    }

    public void Dispose()
    {
        InvokeDispatch(() =>
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