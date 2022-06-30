using System.Threading;

namespace NAPS2.Threading;

public class SmoothProgress : IDisposable
{
    private const int INTERVAL = 16;
    private const int VELOCITY_SAMPLE_SIZE = 5;

    private double _inputPos;
    private double _outputPos;

    private double _inputVelocity;
    private double _timeToCompletion;
    private double _outputVelocity;
    private long _outputLastUpdated;

    private Stopwatch _stopwatch = null!;
    private Timer? _timer;

    private LinkedList<double> _previousInputPos = null!;
    private LinkedList<long> _previousInputTimes = null!;

    public SmoothProgress()
    {
        Reset();
    }

    public void Reset()
    {
        lock (this)
        {
            _inputPos = 0;
            _outputPos = 0;
            InvokeOutputProgressChanged();

            _timer?.Dispose();
            _timer = null;

            _stopwatch = Stopwatch.StartNew();

            _previousInputPos = new LinkedList<double>();
            _previousInputPos.AddLast(0);
            _previousInputTimes = new LinkedList<long>();
            _previousInputTimes.AddLast(0);
        }
    }

    public void InputProgressChanged(double value)
    {
        lock (this)
        {
            if (_inputPos < value)
            {
                _inputPos = value;
                _previousInputPos.AddLast(_inputPos);
                _previousInputTimes.AddLast(_stopwatch.ElapsedMilliseconds);

                var deltaPos = _previousInputPos.Last.Value - SampleStart(_previousInputPos);
                var deltaTime = _previousInputTimes.Last.Value - SampleStart(_previousInputTimes);

                if (deltaTime > 0 && _inputPos < 1)
                {
                    _inputVelocity = deltaPos / deltaTime;
                    _timeToCompletion = (1 - _inputPos) / _inputVelocity;
                    _outputVelocity = (1 - _outputPos) / _timeToCompletion;
                }

                if (_inputPos >= 1)
                {
                    _inputVelocity = 0;
                    _timeToCompletion = 0;
                    _outputVelocity = 1;
                }

                if (_timer == null)
                {
                    _outputLastUpdated = _stopwatch.ElapsedMilliseconds;
                    _timer = new Timer(TimerTick, null, 0, INTERVAL);
                }
            }
        }
    }

    private T SampleStart<T>(LinkedList<T> list)
    {
        var node = list.Last;
        for (int i = 0; i < VELOCITY_SAMPLE_SIZE; i++)
        {
            if (node.Previous == null)
            {
                break;
            }

            node = node.Previous;
        }
        if (node.Previous != null)
        {
            list.RemoveFirst();
        }
        return node.Value;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void TimerTick(object state)
    {
        lock (this)
        {
            var previousUpdateTime = _outputLastUpdated;
            _outputLastUpdated = _stopwatch.ElapsedMilliseconds;
            var interval = _outputLastUpdated - previousUpdateTime;
            if (interval <= 0)
            {
                interval = INTERVAL;
            }
            _outputPos = Math.Min(1.0, _outputPos + _outputVelocity * interval);
        }
        InvokeOutputProgressChanged();
    }

    private void InvokeOutputProgressChanged()
    {
        OutputProgressChanged?.Invoke(this, new ProgressChangeEventArgs(_outputPos));
    }

    public event ProgressChangeEventHandle? OutputProgressChanged;

    public delegate void ProgressChangeEventHandle(object sender, ProgressChangeEventArgs args);

    public class ProgressChangeEventArgs : EventArgs
    {
        public ProgressChangeEventArgs(double value)
        {
            Value = value;
        }

        public double Value { get; set; }
    }
}