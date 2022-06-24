using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class ScopedConfigTests
{
    private readonly ScopedConfig _config = ScopedConfig.Stub(); 
    
    [Fact]
    public void StartsWithInternalDefaults()
    {
        Assert.Equal("en", _config.Get(c => c.Culture));
        Assert.Equal("NAPS2", _config.Get(c => c.PdfSettings.Metadata.Creator));
    }

    [Fact]
    public void GetNonConfigPropertyThrows()
    {
        var ex = Assert.Throws<ArgumentException>(() => _config.Get(c => c.Culture.Length));
        Assert.Contains("[Config]", ex.Message);
    }

    [Fact]
    public void ScopePriority()
    {
        Assert.Equal("en", _config.Get(c => c.Culture));
        
        _config.AppDefault.Set(c => c.Culture, "fr");
        Assert.Equal("fr", _config.Get(c => c.Culture));
        
        _config.User.Set(c => c.Culture, "de");
        Assert.Equal("de", _config.Get(c => c.Culture));
        
        _config.Run.Set(c => c.Culture, "zh");
        Assert.Equal("zh", _config.Get(c => c.Culture));

        _config.AppLocked.Set(c => c.Culture, "hi");
        Assert.Equal("hi", _config.Get(c => c.Culture));
    }

    [Fact]
    public void GetChildConfig()
    {
        _config.AppDefault.Set(c => c.PdfSettings.Metadata.Author, "a");
        _config.User.Set(c => c.PdfSettings.Metadata.Title, "b");
        _config.AppLocked.Set(c => c.PdfSettings.Metadata.Keywords, null);
        _config.Run.Set(c => c.PdfSettings.Metadata.Subject, "c");
        _config.AppDefault.Set(c => c.PdfSettings.DefaultFileName, null);
        _config.User.Set(c => c.ThumbnailSize, 42);

        var metadata = _config.Get(c => c.PdfSettings.Metadata);
        Assert.Equal("a", metadata.Author);
        Assert.Equal("b", metadata.Title);
        Assert.Null(metadata.Keywords);
        Assert.Equal("c", metadata.Subject);
        Assert.Equal("NAPS2", metadata.Creator);

        var pdfSettings = _config.Get(c => c.PdfSettings);
        Assert.Equal("a", pdfSettings.Metadata.Author);
        Assert.Equal("b", pdfSettings.Metadata.Title);
        Assert.Null(pdfSettings.Metadata.Keywords);
        Assert.Equal("c", pdfSettings.Metadata.Subject);
        Assert.Equal("NAPS2", pdfSettings.Metadata.Creator);
        Assert.Null(pdfSettings.DefaultFileName);

        var commonConfig = _config.Get(c => c);
        Assert.Equal("a", commonConfig.PdfSettings.Metadata.Author);
        Assert.Equal("b", commonConfig.PdfSettings.Metadata.Title);
        Assert.Null(commonConfig.PdfSettings.Metadata.Keywords);
        Assert.Equal("c", commonConfig.PdfSettings.Metadata.Subject);
        Assert.Equal("NAPS2", commonConfig.PdfSettings.Metadata.Creator);
        Assert.Null(commonConfig.PdfSettings.DefaultFileName);
        Assert.Equal(42, commonConfig.ThumbnailSize);
    }
    
    // TODO: WithTransaction test
}