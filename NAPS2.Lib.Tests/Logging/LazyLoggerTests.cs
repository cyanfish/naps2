using Microsoft.Extensions.Logging;
using Xunit;

namespace NAPS2.Lib.Tests.Logging;

public class LazyLoggerTests
{
    private readonly RecordingLogger _inner = new();
    private int _createCount;
    private bool _debugEnabled;

    private LazyLogger CreateLogger() => new(() =>
    {
        _createCount++;
        return _inner;
    }, level => level >= LogLevel.Information || _debugEnabled);

    [Fact]
    public void DisabledLevelsDontCreateInner()
    {
        var logger = CreateLogger();

        logger.LogDebug("debug");
        logger.LogTrace("trace");
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.True(logger.IsEnabled(LogLevel.Error));

        Assert.Equal(0, _createCount);
        Assert.Empty(_inner.Messages);
    }

    [Fact]
    public void EnabledLevelsCreateInnerOnce()
    {
        var logger = CreateLogger();

        logger.LogInformation("info");
        logger.LogError("error");

        Assert.Equal(1, _createCount);
        Assert.Equal([(LogLevel.Information, "info"), (LogLevel.Error, "error")], _inner.Messages);
    }

    [Fact]
    public void ChecksDebugEnabledOnEachCall()
    {
        var logger = CreateLogger();

        logger.LogDebug("before");
        _debugEnabled = true;
        logger.LogDebug("enabled");
        _debugEnabled = false;
        logger.LogDebug("after");

        Assert.Equal([(LogLevel.Debug, "enabled")], _inner.Messages);
    }

    [Fact]
    public void NoneIsNeverEnabled()
    {
        var logger = new LazyLogger(() => _inner, _ => true);

        Assert.False(logger.IsEnabled(LogLevel.None));
    }

    [Fact]
    public void NLogConfigLoggerIsLazy()
    {
        // Only checks levels, as logging an enabled message would write to the real log files
        var logger = NLogConfig.CreateLogger(() => false);

        Assert.IsType<LazyLogger>(logger);
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.True(NLogConfig.CreateLogger(() => true).IsEnabled(LogLevel.Debug));
    }

    private class RecordingLogger : ILogger
    {
        public List<(LogLevel, string)> Messages { get; } = [];

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Messages.Add((logLevel, formatter(state, exception)));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
