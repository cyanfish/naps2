using System.Text;
using Xunit.Abstractions;

namespace NAPS2.Lib.Tests.Automation;

internal class TestOutputTextWriter : TextWriter
{
    private readonly StringBuilder _stringBuilder = new();
    private readonly ITestOutputHelper _output;

    public TestOutputTextWriter(ITestOutputHelper output)
    {
        _output = output;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public string Output => _stringBuilder.ToString();

    public override void WriteLine(string message)
    {
        _stringBuilder.AppendLine(message);
        _output.WriteLine(message);
    }

    public override void WriteLine(string format, params object[] args) =>
        WriteLine(string.Format(format, args));
}