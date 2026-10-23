using NAPS2.ImportExport;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class PlaceholdersTests : ContextualTests
{
    private static readonly DateTime TestDate = new(2024, 3, 7, 21, 5, 2);

    private readonly Placeholders _placeholders = Placeholders.All.WithDate(TestDate);

    [Fact]
    public void NullInput()
    {
        Assert.Null(Placeholders.All.Substitute(null));
        Assert.Null(Placeholders.NonNumeric.Substitute(null));
        Assert.Null(Placeholders.Env.Substitute(null));
        Assert.Null(Placeholders.None.Substitute(null));
    }

    [Fact]
    public void DateAndTime()
    {
        Assert.Equal("2024 24 03 07 21 05 02",
            _placeholders.Substitute("$(YYYY) $(YY) $(MM) $(DD) $(hh) $(mm) $(ss)"));
        Assert.Equal("2024-03-07 21_05_02",
            _placeholders.Substitute($"{Placeholders.FULL_DATE} {Placeholders.FULL_TIME}"));
    }

    [Fact]
    public void DateInDirectory()
    {
        var path = Path.Combine(FolderPath, "$(YYYY)", "$(MM)", "scan.pdf");
        Assert.Equal(Path.Combine(FolderPath, "2024", "03", "scan.pdf"), _placeholders.Substitute(path));
    }

    [Fact]
    public void UnknownPlaceholderUnchanged()
    {
        Assert.Equal("$(abc) $(yyyy)", _placeholders.Substitute("$(abc) $(yyyy)"));
    }

    [Fact]
    public void EnvironmentVariables()
    {
        var name = "NAPS2_TEST_" + Path.GetRandomFileName().Replace(".", "");
        Environment.SetEnvironmentVariable(name, "value");
        try
        {
            Assert.Equal("value_2024", _placeholders.Substitute($"%{name}%_$(YYYY)"));
            Assert.Equal("value_2024", Placeholders.NonNumeric.WithDate(TestDate).Substitute($"%{name}%_$(YYYY)"));
            Assert.Equal("value_$(YYYY)", Placeholders.Env.Substitute($"%{name}%_$(YYYY)"));
            Assert.Equal($"%{name}%_$(YYYY)", Placeholders.None.Substitute($"%{name}%_$(YYYY)"));
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
        var path = Path.Combine(FolderPath, $"scan{placeholder}.pdf");
        Assert.Equal(Path.Combine(FolderPath, expected), _placeholders.Substitute(path));
    }

    [Fact]
    public void NumberPlaceholder_IncrementsPastExistingFiles()
    {
        File.WriteAllText(Path.Combine(FolderPath, "scan001.pdf"), "");
        File.WriteAllText(Path.Combine(FolderPath, "scan002.pdf"), "");
        File.WriteAllText(Path.Combine(FolderPath, "scan004.pdf"), "");

        var path = Path.Combine(FolderPath, "scan$(nnn).pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan003.pdf"), _placeholders.Substitute(path));
    }

    [Fact]
    public void NumberPlaceholder_IncrementsEvenIfIncrementIfExistsIsFalse()
    {
        File.WriteAllText(Path.Combine(FolderPath, "scan001.pdf"), "");

        // incrementIfExists only applies when there isn't a numeric placeholder
        var path = Path.Combine(FolderPath, "scan$(nnn).pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan002.pdf"),
            _placeholders.Substitute(path, incrementIfExists: false));
    }

    [Fact]
    public void NumberPlaceholder_NumberSkip()
    {
        var path = Path.Combine(FolderPath, "scan$(nnn).pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan005.pdf"), _placeholders.Substitute(path, numberSkip: 4));
    }

    [Fact]
    public void NumberPlaceholder_ExceedsDigits()
    {
        var path = Path.Combine(FolderPath, "scan$(n).pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan10.pdf"), _placeholders.Substitute(path, numberSkip: 9));
    }

    [Fact]
    public void NumberPlaceholder_IgnoresAutoNumberDigits()
    {
        var path = Path.Combine(FolderPath, "scan$(n).pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan1.pdf"), _placeholders.Substitute(path, autoNumberDigits: 3));
    }

    [Fact]
    public void NumberPlaceholder_WithDate()
    {
        var path = Path.Combine(FolderPath, "$(YYYY)-$(MM)-$(DD)_$(nn).pdf");
        Assert.Equal(Path.Combine(FolderPath, "2024-03-07_01.pdf"), _placeholders.Substitute(path));
    }

    [Fact]
    public void AutoNumber()
    {
        var path = Path.Combine(FolderPath, "scan.pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan.001.pdf"), _placeholders.Substitute(path, autoNumberDigits: 3));
        Assert.Equal(Path.Combine(FolderPath, "scan.3.pdf"),
            _placeholders.Substitute(path, numberSkip: 2, autoNumberDigits: 1));
    }

    [Fact]
    public void AutoNumber_IncrementsPastExistingFiles()
    {
        File.WriteAllText(Path.Combine(FolderPath, "scan.1.pdf"), "");

        var path = Path.Combine(FolderPath, "scan.pdf");
        Assert.Equal(Path.Combine(FolderPath, "scan.2.pdf"), _placeholders.Substitute(path, autoNumberDigits: 1));
        Assert.Equal(Path.Combine(FolderPath, "scan.1.pdf"),
            _placeholders.Substitute(path, incrementIfExists: false, autoNumberDigits: 1));
    }

    [Fact]
    public void AutoNumber_NoExtension()
    {
        var path = Path.Combine(FolderPath, "scan");
        Assert.Equal(Path.Combine(FolderPath, "scan.01"), _placeholders.Substitute(path, autoNumberDigits: 2));
    }

    [Fact]
    public void NoNumber_WithoutPlaceholderOrAutoNumber()
    {
        File.WriteAllText(Path.Combine(FolderPath, "scan.pdf"), "");

        var path = Path.Combine(FolderPath, "scan.pdf");
        Assert.Equal(path, _placeholders.Substitute(path));
    }

    [Fact]
    public void NonNumeric_LeavesNumberPlaceholders()
    {
        var placeholders = Placeholders.NonNumeric.WithDate(TestDate);
        Assert.Equal("scan_2024_$(nnn).pdf", placeholders.Substitute("scan_$(YYYY)_$(nnn).pdf"));
        Assert.Equal("scan.pdf", placeholders.Substitute("scan.pdf", autoNumberDigits: 3));
    }
}
