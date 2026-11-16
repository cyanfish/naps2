#if TEST_SCENARIOS
namespace NAPS2.TestScenarios;

/// <summary>
/// Replaces the system's "open with" applications in test scenarios with TestScenarioSetup.FakeOpenWith (if set).
/// </summary>
internal class FakeOpenWith(IOpenWith inner, TestScenarioSetup setup) : IOpenWith
{
    public IEnumerable<OpenWithEntry> GetEntries(string fileExt) => setup.FakeOpenWith ?? inner.GetEntries(fileExt);

    public void OpenWith(string entryId, IEnumerable<string> filePaths)
    {
        if (setup.FakeOpenWith == null)
        {
            inner.OpenWith(entryId, filePaths);
        }
    }

    public IMemoryImage? LoadIcon(OpenWithEntry entry) => setup.FakeOpenWith != null ? null : inner.LoadIcon(entry);
}
#endif
