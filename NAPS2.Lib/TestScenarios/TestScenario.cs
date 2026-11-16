#if TEST_SCENARIOS
namespace NAPS2.TestScenarios;

/// <summary>
/// A sequence of UI actions run in the real application (see TestScenarioModule).
/// </summary>
/// <param name="Setup">Configures state before the application starts.</param>
/// <param name="Run">Runs on a background thread once the main window is shown.</param>
public record TestScenario(Action<TestScenarioSetup>? Setup, Func<TestScenarioContext, Task> Run);
#endif
