using NAPS2.ImportExport;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.ImportExport;

public class NameManagerTests : ContextualTests
{
    private static readonly DateTime TestDate = new(2024, 3, 7, 21, 5, 2);

    private readonly NameManager _nameManager = new();
    private readonly NameContext _context = new() { DateTime = TestDate };

    private string Get(string input) => _nameManager.Get(input, _context);

    private string Get(string input, NameContext context) => _nameManager.Get(input, context);

    private NameContext AlwaysContext => _context with { Numbering = Numbering.Always };

    private NameContext OnConflictContext => _context with { Numbering = Numbering.OnConflict };

    [Fact]
    public void NullInput()
    {
        Assert.Null(_nameManager.Get(null, _context));
    }

    [Fact]
    public void DateAndTime()
    {
        Assert.Equal("2024 24 03 07 21 05 02", Get("$(YYYY) $(YY) $(MM) $(DD) $(hh) $(mm) $(ss)"));
        Assert.Equal("2024-03-07 21_05_02", Get($"{NameManager.FULL_DATE} {NameManager.FULL_TIME}"));
    }

    [Fact]
    public void DateInDirectory()
    {
        Assert.Equal(P("2024", "03", "scan.pdf"), Get(P("$(YYYY)", "$(MM)", "scan.pdf")));
    }

    [Fact]
    public void UnknownPlaceholderUnchanged()
    {
        Assert.Equal("$(abc) $(yyyy)", Get("$(abc) $(yyyy)"));
    }

    [Fact]
    public void EnvironmentVariables()
    {
        var name = "NAPS2_TEST_" + Path.GetRandomFileName().Replace(".", "");
        Environment.SetEnvironmentVariable(name, "value");
        try
        {
            Assert.Equal("value_2024", Get($"%{name}%_$(YYYY)"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Theory]
    [InlineData("$(n)", "scan1.pdf")]
    [InlineData("$(nn)", "scan01.pdf")]
    [InlineData("$(nnn)", "scan001.pdf")]
    [InlineData("$(nnnn)", "scan0001.pdf")]
    public void NumberPlaceholder(string placeholder, string expected)
    {
        Assert.Equal(P(expected), Get(P($"scan{placeholder}.pdf")));
    }

    [Fact]
    public void NumberPlaceholder_SkipsExistingFiles()
    {
        File.WriteAllText(P("scan001.pdf"), "");
        File.WriteAllText(P("scan002.pdf"), "");
        File.WriteAllText(P("scan004.pdf"), "");

        Assert.Equal(P("scan003.pdf"), Get(P("scan$(nnn).pdf")));
    }

    [Fact]
    public void NumberPlaceholder_BatchIndex()
    {
        Assert.Equal(P("scan005.pdf"), Get(P("scan$(nnn).pdf"), _context with { BatchIndex = 4 }));
    }

    [Fact]
    public void NumberPlaceholder_ExceedsDigits()
    {
        Assert.Equal(P("scan10.pdf"), Get(P("scan$(n).pdf"), _context with { BatchIndex = 9 }));
    }

    [Fact]
    public void NumberPlaceholder_IgnoresBatchCount()
    {
        Assert.Equal(P("scan1.pdf"), Get(P("scan$(n).pdf"), AlwaysContext with { BatchCount = 100 }));
    }

    [Fact]
    public void NumberPlaceholder_WithDate()
    {
        Assert.Equal(P("2024-03-07_01.pdf"), Get(P("$(YYYY)-$(MM)-$(DD)_$(nn).pdf")));
    }

    [Fact]
    public void NumberPlaceholder_ExtraPlaceholdersRemoved()
    {
        Assert.Equal(P("scan1_.pdf"), Get(P("scan$(n)_$(nn).pdf")));
    }

    [Fact]
    public void Always()
    {
        Assert.Equal(P("scan.1.pdf"), Get(P("scan.pdf"), AlwaysContext));
        Assert.Equal(P("scan.001.pdf"), Get(P("scan.pdf"), AlwaysContext with { BatchCount = 100 }));
        Assert.Equal(P("scan.3.pdf"), Get(P("scan.pdf"), AlwaysContext with { BatchIndex = 2 }));
    }

    [Fact]
    public void Always_SkipsExistingFiles()
    {
        File.WriteAllText(P("scan.1.pdf"), "");

        Assert.Equal(P("scan.2.pdf"), Get(P("scan.pdf"), AlwaysContext));
    }

    [Fact]
    public void Always_NoExtension()
    {
        Assert.Equal(P("scan.01"), Get(P("scan"), AlwaysContext with { BatchCount = 10 }));
    }

    [Fact]
    public void Placeholder_NoNumberWithoutPlaceholder()
    {
        File.WriteAllText(P("scan.pdf"), "");

        Assert.Equal(P("scan.pdf"), Get(P("scan.pdf")));
    }

    [Fact]
    public void NotFilePath_DoesNotCheckFiles()
    {
        File.WriteAllText(P("scan1.pdf"), "");
        File.WriteAllText(P("scan.1.pdf"), "");
        var context = _context with { IsFilePath = false };

        Assert.Equal(P("scan1.pdf"), Get(P("scan$(n).pdf"), context));
        Assert.Equal(P("scan.1.pdf"), Get(P("scan.pdf"), context with { Numbering = Numbering.Always }));
        Assert.Equal(P("scan.03.pdf"),
            Get(P("scan.pdf"), context with { Numbering = Numbering.Always, BatchIndex = 2, BatchCount = 10 }));
    }

    [Fact]
    public void None()
    {
        var context = _context with { Numbering = Numbering.None };

        Assert.Equal("scan_2024_$(nnn).pdf", Get("scan_$(YYYY)_$(nnn).pdf", context));
        Assert.Equal("scan.pdf", Get("scan.pdf", context));
    }

    [Fact]
    public void OnConflict()
    {
        Assert.Equal(P("scan.pdf"), Get(P("scan.pdf"), OnConflictContext));

        File.WriteAllText(P("scan.pdf"), "");
        Assert.Equal(P("scan.1.pdf"), Get(P("scan.pdf"), OnConflictContext));

        File.WriteAllText(P("scan.1.pdf"), "");
        Assert.Equal(P("scan.2.pdf"), Get(P("scan.pdf"), OnConflictContext));
    }

    [Fact]
    public void OnConflict_NumberPlaceholder()
    {
        File.WriteAllText(P("scan1.pdf"), "");

        Assert.Equal(P("scan2.pdf"), Get(P("scan$(n).pdf"), OnConflictContext));
    }

    [Fact]
    public void OnConflict_NotFilePath()
    {
        File.WriteAllText(P("scan.pdf"), "");

        Assert.Equal(P("scan.pdf"), Get(P("scan.pdf"), OnConflictContext with { IsFilePath = false }));
    }

    [Fact]
    public void OnConflict_Reserved()
    {
        using var reservation = _nameManager.Reserve(P("scan.pdf"), _context);
        Assert.Equal(P("scan.pdf"), reservation.Name);

        Assert.Equal(P("scan.1.pdf"), Get(P("scan.pdf"), OnConflictContext));
    }

    [Fact]
    public void Reserve_SkipsReservedNames()
    {
        using var reservation1 = _nameManager.Reserve(P("scan$(n).pdf"), _context);
        using var reservation2 = _nameManager.Reserve(P("scan$(n).pdf"), _context);
        using var reservation3 = _nameManager.Reserve(P("scan.pdf"), AlwaysContext);

        Assert.Equal(P("scan1.pdf"), reservation1.Name);
        Assert.Equal(P("scan2.pdf"), reservation2.Name);
        Assert.Equal(P("scan.1.pdf"), reservation3.Name);
        Assert.Equal(P("scan3.pdf"), Get(P("scan$(n).pdf")));
        Assert.Equal(P("scan.2.pdf"), Get(P("scan.pdf"), AlwaysContext));
    }

    [Fact]
    public void Reserve_DisposeReleasesName()
    {
        var reservation = _nameManager.Reserve(P("scan$(n).pdf"), _context);
        Assert.Equal(P("scan1.pdf"), reservation.Name);

        reservation.Dispose();

        Assert.Equal(P("scan1.pdf"), Get(P("scan$(n).pdf")));
    }

    [Fact]
    public void Reserve_SameNameTwice()
    {
        // e.g. two operations saving to the same fixed file name; the name stays reserved until both are done
        var reservation1 = _nameManager.Reserve(P("scan1.pdf"), _context);
        var reservation2 = _nameManager.Reserve(P("scan1.pdf"), _context);

        reservation1.Dispose();
        Assert.Equal(P("scan2.pdf"), Get(P("scan$(n).pdf")));

        reservation2.Dispose();
        Assert.Equal(P("scan1.pdf"), Get(P("scan$(n).pdf")));
    }

    [Fact]
    public void Reserve_DifferentCaseOrRelativePath()
    {
        var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, P("scan1.pdf"));
        using var reservation = _nameManager.Reserve(relativePath, _context);

        if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            Assert.Equal(P("Scan2.pdf"), Get(P("Scan$(n).pdf")));
        }
        else
        {
            Assert.Equal(P("scan2.pdf"), Get(P("scan$(n).pdf")));
        }
    }

    [Fact]
    public void Reserve_NotFilePath()
    {
        using var reservation = _nameManager.Reserve("scan$(n).pdf", _context with { IsFilePath = false });
        Assert.Equal("scan1.pdf", reservation.Name);

        Assert.Equal("scan1.pdf", Get("scan$(n).pdf", _context with { IsFilePath = false }));
    }

    [Fact]
    public void Overwrite_DoesNotSkipExistingFiles()
    {
        File.WriteAllText(P("scan1.pdf"), "");
        File.WriteAllText(P("scan.1.pdf"), "");
        var context = _context with { Overwrite = true };

        Assert.Equal(P("scan1.pdf"), Get(P("scan$(n).pdf"), context));
        Assert.Equal(P("scan.1.pdf"), Get(P("scan.pdf"), context with { Numbering = Numbering.Always }));
    }

    [Fact]
    public void Overwrite_SkipsReservedNames()
    {
        var context = _context with { Overwrite = true };
        using var reservation = _nameManager.Reserve(P("scan$(n).pdf"), context);
        Assert.Equal(P("scan1.pdf"), reservation.Name);

        Assert.Equal(P("scan2.pdf"), Get(P("scan$(n).pdf"), context));
    }

    [Fact]
    public void Overwrite_BatchIndex()
    {
        File.WriteAllText(P("scan1.pdf"), "");
        File.WriteAllText(P("scan2.pdf"), "");

        Assert.Equal(P("scan2.pdf"), Get(P("scan$(n).pdf"), _context with { Overwrite = true, BatchIndex = 1 }));
    }
}
