#if TEST_SCENARIOS
using NAPS2.Update;

namespace NAPS2.TestScenarios;

/// <summary>
/// Replaces update checks in test scenarios with TestScenarioSetup.FakeUpdate so there's no network access.
/// </summary>
internal class FakeUpdateChecker(IUpdateChecker inner, TestScenarioSetup setup) : IUpdateChecker
{
    public Task<UpdateInfo?> CheckForUpdates() =>
        setup.FakeUpdateCheckNeverCompletes
            ? new TaskCompletionSource<UpdateInfo?>().Task
            : Task.FromResult(setup.FakeUpdate);

    public UpdateOperation StartUpdate(UpdateInfo update) => inner.StartUpdate(update);
}
#endif
