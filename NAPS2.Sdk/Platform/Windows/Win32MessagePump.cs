using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NAPS2.Platform.Windows;

internal class Win32MessagePump : IInvoker, IDisposable
{
    private const string WND_CLASS_NAME = "MPWndClass";
    private const string RUN_QUEUED_ACTIONS_MESSAGE_NAME = "MPRunQueuedActions";

    public static Win32MessagePump Create()
    {
        return new Win32MessagePump();
    }

    // We store the delegate as an instance variable so it doesn't get garbage collected
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Win32.WndProc _wndProcDelegate;
    private readonly uint _runQueuedActionsMessage;

    private readonly Queue<Action> _queue = new();
    private readonly Thread? _messageLoopThread;
    private bool _stopped;

    private Win32MessagePump()
    {
        _messageLoopThread = Thread.CurrentThread;
        _wndProcDelegate = CustomWndProc;

        Win32.RegisterClassW(new Win32.WndClass
        {
            lpszClassName = WND_CLASS_NAME,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = Process.GetCurrentProcess().Handle
        });

        _runQueuedActionsMessage = Win32.RegisterWindowMessage(RUN_QUEUED_ACTIONS_MESSAGE_NAME);

        Handle = Win32.CreateWindowEx(0, WND_CLASS_NAME, "", 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero,
            Process.GetCurrentProcess().Handle, IntPtr.Zero);
    }

    public Func<IntPtr, int, IntPtr, IntPtr, bool>? Filter { get; set; }

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
            Win32.PostMessage(Handle, _runQueuedActionsMessage, IntPtr.Zero, IntPtr.Zero);
        }
        toggle.WaitOne();
    }

    public void InvokeDispatch(Action action)
    {
        lock (_queue)
        {
            _queue.Enqueue(action);
            Win32.PostMessage(Handle, _runQueuedActionsMessage, IntPtr.Zero, IntPtr.Zero);
        }
    }

    public T InvokeGet<T>(Func<T> func)
    {
        T value = default!;
        Invoke(() => value = func());
        return value;
    }

    public void RunMessageLoop()
    {
        try
        {
            while (!_stopped && Win32.GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
            {
                if (!(Filter?.Invoke(Handle, msg.msg, msg.wParam, msg.lParam) ?? false))
                {
                    Win32.TranslateMessage(msg);
                    Win32.DispatchMessage(ref msg);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in message loop");
        }
    }

    private void RunQueuedActions()
    {
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
                Logger.LogError(ex, "Error in invoked action");
            }
        }
    }

    private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == _runQueuedActionsMessage)
        {
            RunQueuedActions();
            return IntPtr.Zero;
        }
        return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    public IntPtr CreateBackgroundWindow(IntPtr parent = default)
    {
        return InvokeGet(() =>
            Win32.CreateWindowEx(0, WND_CLASS_NAME, "", 0, 0, 0, 0, 0, parent, IntPtr.Zero,
                Process.GetCurrentProcess().Handle, IntPtr.Zero));
    }

    public void CloseWindow(IntPtr window)
    {
        InvokeDispatch(() => Win32.DestroyWindow(window));
    }

    public void Dispose()
    {
        InvokeDispatch(() =>
        {
            Win32.DestroyWindow(Handle);
            _stopped = true;
        });
    }
}