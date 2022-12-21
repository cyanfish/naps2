namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// As Twain doesn't provide real progress data, this class maintains a static cache of past scan timing to estimate
/// how long future page scans will take in order to give a useful progress bar.
///
/// For Twain memory buffer scans we do get some progress in the form of the number of pixels transferred, and that is
/// incorporated into the calculations.
///
/// The mathematical model used is that there is some fixed overhead time at the start of the scan, then a fixed bitrate
/// to transfer all the pixels.  
/// </summary>
internal class TwainProgressEstimator
{
    private readonly IScanEvents _scanEvents;
    private readonly TimingCache _timingCache;
    private readonly TimingKey _key;
    
    private long _timeToFirstBuffer;
    private long _pixelsAtFirstBuffer;
    private long _totalPixels;
    private TimingInfo? _previousTimingInfo;
    private Stopwatch? _stopwatch;

    public static bool HasTimingInfo(ScanOptions options)
    {
        return TimingCache.Instance.Read(GetTimingKey(options)) != null;
    }

    private static TimingKey GetTimingKey(ScanOptions options)
    {
        return new TimingKey(options.Device!.Id!, options.BitDepth, options.PageSize!);
    }

    public TwainProgressEstimator(ScanOptions options, IScanEvents scanEvents)
        : this(options, scanEvents, TimingCache.Instance)
    {
    }

    public TwainProgressEstimator(ScanOptions options, IScanEvents scanEvents, TimingCache timingCache)
    {
        _scanEvents = scanEvents;
        _timingCache = timingCache;
        _key = GetTimingKey(options);
    }

    public void MarkStart(long totalPixels)
    {
        lock (this)
        {
            _stopwatch = Stopwatch.StartNew();
            _timeToFirstBuffer = -1;
            _pixelsAtFirstBuffer = -1;
            _totalPixels = totalPixels;
            _scanEvents.PageStart();

            _previousTimingInfo = _timingCache.Read(_key);
            if (_previousTimingInfo != null)
            {
                Task.Delay(200).ContinueWith(_ => SendEstimatedProgress());
                Task.Delay(1000).ContinueWith(_ => SendEstimatedProgress());
            }
        }
    }

    private void SendEstimatedProgress()
    {
        lock (this)
        {
            if (_timeToFirstBuffer == -1 && _stopwatch != null && _previousTimingInfo != null)
            {
                _scanEvents.PageProgress(_stopwatch.ElapsedMilliseconds / (double) _previousTimingInfo.TotalMillis);
            }
        }
    }

    public void MarkCompletion()
    {
        lock (this)
        {
            if (_stopwatch == null)
            {
                throw new InvalidOperationException();
            }
            if (_totalPixels <= 0 || _pixelsAtFirstBuffer == _totalPixels)
            {
                _timingCache.Add(_key, new TimingInfo(0, _stopwatch.ElapsedMilliseconds));
                return;
            }
            var totalTime = _stopwatch.ElapsedMilliseconds;
            var timeSinceFirstBuffer = totalTime - _timeToFirstBuffer;
            var pixelsSinceFirstBuffer = _totalPixels - _pixelsAtFirstBuffer;
            var pixelRate = pixelsSinceFirstBuffer / (double) timeSinceFirstBuffer;
            var overheadOffset = (int) (_pixelsAtFirstBuffer / pixelRate);
            var overhead = Math.Max(0, _timeToFirstBuffer - overheadOffset);
            _timingCache.Add(_key, new TimingInfo(overhead, totalTime));
        }
    }

    public void MarkProgress(long transferredPixels, long totalPixels)
    {
        lock (this)
        {
            if (_stopwatch == null)
            {
                throw new InvalidOperationException();
            }
            if (_timeToFirstBuffer == -1)
            {
                _timeToFirstBuffer = _stopwatch.ElapsedMilliseconds;
                _pixelsAtFirstBuffer = transferredPixels;
            }
            var progress = transferredPixels / (double) totalPixels;
            if (_previousTimingInfo != null)
            {
                var overheadDone = _previousTimingInfo.OverheadMillis / (double) _previousTimingInfo.TotalMillis;
                var nonOverheadRatio = (_previousTimingInfo.TotalMillis - _previousTimingInfo.OverheadMillis) /
                                       _previousTimingInfo.TotalMillis;
                progress = overheadDone + progress * nonOverheadRatio;
            }
            _scanEvents.PageProgress(progress);
        }
    }

    public class TimingCache
    {
        public static TimingCache Instance = new();
        
        public void Add(TimingKey key, TimingInfo value)
        {
            lock (_cache)
            {
                _cache[key] = value;
            }
        }

        public TimingInfo? Read(TimingKey key)
        {
            lock (_cache)
            {
                return _cache.Get(key);
            }
        }

        private readonly Dictionary<TimingKey, TimingInfo> _cache = new();
    }

    public record TimingKey(string DeviceId, BitDepth BitDepth, PageSize PageSize);

    public record TimingInfo(long OverheadMillis, long TotalMillis);
}