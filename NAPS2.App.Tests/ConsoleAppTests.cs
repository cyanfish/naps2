using NAPS2.App.Tests.Targets;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.App.Tests;

public class ConsoleAppTests : ContextualTests
{
    [Theory]
    [ClassData(typeof(AppTestData))]
    public void ConvertsImportedFile(IAppTestTarget target)
    {
        var importPath = CopyResourceToFile(ImageResources.dog, "in.png");
        var outputPath = Path.Combine(FolderPath, "out.jpg");
        var args = $"-n 0 -i \"{importPath}\" -o \"{outputPath}\"";

        var process = AppTestHelper.StartProcess(target.Console, FolderPath, args);
        try
        {
            Assert.True(process.WaitForExit(5000));
            var stdout = process.StandardOutput.ReadToEnd();
            Assert.Equal(0, process.ExitCode);
            Assert.Empty(stdout);
            AppTestHelper.AssertNoErrorLog(FolderPath);
            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }

    [Theory]
    [ClassData(typeof(AppTestData))]
    public void NonZeroExitCodeForError(IAppTestTarget target)
    {
        var importPath = Path.Combine(FolderPath, "doesnotexist.png");
        var outputPath = Path.Combine(FolderPath, "out.jpg");
        var args = $"-n 0 -i \"{importPath}\" -o \"{outputPath}\"";

        var process = AppTestHelper.StartProcess(target.Console, FolderPath, args);
        try
        {
            Assert.True(process.WaitForExit(5000));
            var stdout = process.StandardOutput.ReadToEnd();
            if (OperatingSystem.IsWindows())
            {
                // TODO: Figure out why ExitCode always appears as 0 on Mac/Linux
                Assert.NotEqual(0, process.ExitCode);
            }
            Assert.NotEmpty(stdout);
            AppTestHelper.AssertErrorLog(FolderPath);
            Assert.False(File.Exists(outputPath));
        }
        finally
        {
            AppTestHelper.Cleanup(process);
        }
    }
}