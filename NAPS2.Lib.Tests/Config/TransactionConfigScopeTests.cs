using NAPS2.Config.Model;
using NAPS2.Sdk.Tests;
using NSubstitute;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class TransactionConfigScopeTests
{
    private readonly ConfigScope<CommonConfig> _baseScope;
    private readonly TransactionConfigScope<CommonConfig> _transact;

    public TransactionConfigScopeTests()
    {
        _baseScope = ConfigScope.Memory<CommonConfig>();
        _baseScope.Set(c => c.Culture, "fr");
        _baseScope.Set(c => c.PdfSettings.DefaultFileName, "test");
        _transact = _baseScope.BeginTransaction();
    }

    [Fact]
    public void ReadsFromUnderlying()
    {
        Assert.False(_transact.HasChanges);
        Assert.Equal("fr", _transact.GetOrDefault(c => c.Culture));
    }

    [Fact]
    public void Commit()
    {
        _transact.Set(c => c.Culture, "de");
        Assert.True(_transact.HasChanges);
        Assert.Equal("fr", _baseScope.GetOrDefault(c => c.Culture));
        Assert.Equal("de", _transact.GetOrDefault(c => c.Culture));

        _transact.Commit();
        Assert.False(_transact.HasChanges);
        Assert.Equal("de", _baseScope.GetOrDefault(c => c.Culture));
        Assert.Equal("de", _transact.GetOrDefault(c => c.Culture));
    }

    [Fact]
    public void Rollback()
    {
        _transact.Set(c => c.Culture, "de");
        Assert.True(_transact.HasChanges);
        Assert.Equal("fr", _baseScope.GetOrDefault(c => c.Culture));
        Assert.Equal("de", _transact.GetOrDefault(c => c.Culture));

        _transact.Rollback();
        Assert.False(_transact.HasChanges);
        Assert.Equal("fr", _baseScope.GetOrDefault(c => c.Culture));
        Assert.Equal("fr", _transact.GetOrDefault(c => c.Culture));
    }

    [Fact]
    public void ChangeEvent()
    {
        var mockHandler = Substitute.For<EventHandler>();
        _transact.HasChangesChanged += mockHandler;
        mockHandler.ReceivedCallsCount(0);

        _transact.Set(c => c.Culture, "de");
        mockHandler.Received()(_transact, EventArgs.Empty);
        _transact.Set(c => c.LastImageExt, ".png");
        mockHandler.ReceivedCallsCount(1);

        _transact.Rollback();
        mockHandler.Received()(_transact, EventArgs.Empty);
        mockHandler.ReceivedCallsCount(2);

        _transact.Set(c => c.Culture, "de");
        mockHandler.Received()(_transact, EventArgs.Empty);
        mockHandler.ReceivedCallsCount(3);

        _transact.Commit();
        mockHandler.Received()(_transact, EventArgs.Empty);
        mockHandler.ReceivedCallsCount(4);
    }

    [Fact]
    public void SetThenRemove()
    {
        _transact.Set(c => c.Culture, "de");
        Assert.True(_transact.Has(c => c.Culture));
        _transact.Remove(c => c.Culture);
        Assert.False(_transact.Has(c => c.Culture));
        Assert.True(_transact.HasChanges);

        _transact.Commit();
        Assert.False(_transact.HasChanges);
        Assert.False(_baseScope.Has(c => c.Culture));
    }

    [Fact]
    public void RemoveThenSet()
    {
        _transact.Remove(c => c.Culture);
        Assert.False(_transact.Has(c => c.Culture));
        _transact.Set(c => c.Culture, "de");
        Assert.True(_transact.Has(c => c.Culture));
        Assert.True(_transact.HasChanges);

        _transact.Commit();
        Assert.False(_transact.HasChanges);
        Assert.Equal("de", _baseScope.GetOrDefault(c => c.Culture));
    }

    [Fact]
    public void RemoveParentThenSet()
    {
        _transact.Remove(c => c.PdfSettings);
        Assert.False(_transact.Has(c => c.PdfSettings.DefaultFileName));
        _transact.Set(c => c.PdfSettings.DefaultFileName, "test");
        Assert.True(_transact.Has(c => c.PdfSettings.DefaultFileName));
        Assert.True(_transact.HasChanges);

        _transact.Commit();
        Assert.False(_transact.HasChanges);
        Assert.Equal("test", _baseScope.GetOrDefault(c => c.PdfSettings.DefaultFileName));
    }

    [Fact]
    public void SetThenRemoveParent()
    {
        _transact.Set(c => c.PdfSettings.DefaultFileName, "test");
        Assert.True(_transact.Has(c => c.PdfSettings.DefaultFileName));
        _transact.Remove(c => c.PdfSettings);
        Assert.False(_transact.Has(c => c.PdfSettings.DefaultFileName));
        Assert.True(_transact.HasChanges);

        _transact.Commit();
        Assert.False(_transact.HasChanges);
        Assert.False(_transact.Has(c => c.PdfSettings.DefaultFileName));
    }
}