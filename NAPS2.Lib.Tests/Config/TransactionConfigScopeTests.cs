using Moq;
using NAPS2.Config.Model;
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
        _transact = _baseScope.BeginTransaction();
    }

    [Fact]
    public void ReadsFromUnderlying()
    {
        Assert.False(_transact.HasChanges);
        Assert.Equal("fr", _transact.GetOr(c => c.Culture, null));
    }

    [Fact]
    public void Commit()
    {
        _transact.Set(c => c.Culture, "de");
        Assert.True(_transact.HasChanges);
        Assert.Equal("fr", _baseScope.GetOr(c => c.Culture, null));
        Assert.Equal("de", _transact.GetOr(c => c.Culture, null));
        
        _transact.Commit();
        Assert.False(_transact.HasChanges);
        Assert.Equal("de", _baseScope.GetOr(c => c.Culture, null));
        Assert.Equal("de", _transact.GetOr(c => c.Culture, null));
    }

    [Fact]
    public void Rollback()
    {
        _transact.Set(c => c.Culture, "de");
        Assert.True(_transact.HasChanges);
        Assert.Equal("fr", _baseScope.GetOr(c => c.Culture, null));
        Assert.Equal("de", _transact.GetOr(c => c.Culture, null));
        
        _transact.Rollback();
        Assert.False(_transact.HasChanges);
        Assert.Equal("fr", _baseScope.GetOr(c => c.Culture, null));
        Assert.Equal("fr", _transact.GetOr(c => c.Culture, null));
    }

    [Fact]
    public void ChangeEvent()
    {
        var mockHandler = new Mock<EventHandler>();
        _transact.HasChangesChanged += mockHandler.Object;
        mockHandler.VerifyNoOtherCalls();
        
        _transact.Set(c => c.Culture, "de");
        mockHandler.Verify(x => x(_transact, EventArgs.Empty));
        _transact.Set(c => c.LastImageExt, ".png");
        mockHandler.VerifyNoOtherCalls();
        
        _transact.Rollback();
        mockHandler.Verify(x => x(_transact, EventArgs.Empty));
        mockHandler.VerifyNoOtherCalls();

        _transact.Set(c => c.Culture, "de");
        mockHandler.Verify(x => x(_transact, EventArgs.Empty));
        mockHandler.VerifyNoOtherCalls();
        
        _transact.Commit();
        mockHandler.Verify(x => x(_transact, EventArgs.Empty));
        mockHandler.VerifyNoOtherCalls();
    }
}