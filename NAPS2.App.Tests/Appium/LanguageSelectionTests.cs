using System.Collections.ObjectModel;
using NAPS2.App.Tests.Verification;
using NAPS2.Sdk.Tests;
using OpenQA.Selenium.Appium.Windows;
using Xunit;

namespace NAPS2.App.Tests.Appium;

public class LanguageSelectionTests : ContextualTexts
{
    // TODO: Verify why zh-TW isn't here (and that hi still hasn't been translated)
    private static readonly HashSet<string> ExpectedMissingLanguages = new() { "zh-TW", "hi" };

    private readonly WindowsDriver<WindowsElement> _session;

    public LanguageSelectionTests()
    {
        _session = AppiumHelper.StartSession("NAPS2.exe", FolderPath);
    }

    public override void Dispose()
    {
        _session.Dispose();
        base.Dispose();
    }

    [VerifyFact]
    public void OpenLanguageDropdown()
    {
        // Open the Language dropdown
        ClickAt(_session.FindElementByName("Language"));
        var menuItems = GetMenuItems();

        // Verify all expected languages have menu items
        var menuItemTexts = menuItems.Select(x => x.Text).ToHashSet();
        var allLanguages = GetAllLanguages();
        var missingLanguages = allLanguages
            .Where(x => !menuItemTexts.Contains(x.langName) && !ExpectedMissingLanguages.Contains(x.langCode)).ToList();
        Assert.True(missingLanguages.Count == 0, $"Missing languages: {string.Join(",", missingLanguages)}");

        // Verify French (fr) translation as a standard language example
        ClickAt(_session.FindElementByName("Français"));
        ClickAt(_session.FindElementByName("Langue"));
        
        // Verify Portuguese (pt-BR) translation as a country-specific language example
        ClickAt(_session.FindElementByName("Português (Brasil)"));
        ClickAt(_session.FindElementByName("Idioma"));
        
        // Verify Hebrew translation as a RTL language example
        ClickAt(_session.FindElementByName("עברית"));
        // Toggling RTL causes a new window to be created
        ResetMainWindow();
        ClickAt(_session.FindElementByName("שפה"));

        // And back to English
        ClickAt(_session.FindElementByName("English"));
        ResetMainWindow();
        ClickAt(_session.FindElementByName("Language"));
    }

    private void ResetMainWindow()
    {
        _session.SwitchTo().Window(_session.WindowHandles[0]);
    }

    private void ClickAt(WindowsElement element)
    {
        _session.Mouse.Click(element.Coordinates);
    }

    private List<(string langCode, string langName)> GetAllLanguages()
    {
        return new CultureHelper(Naps2Config.Stub()).GetAllCultures().ToList();
    }

    private ReadOnlyCollection<WindowsElement> GetMenuItems()
    {
        return _session.FindElementsByTagName("MenuItem");
    }

    // TODO: As part of language switching, check several languages (one LTR: fr, one RTL: he, one country-specific LTR: pt-BR); and check several strings (from each NAPS2.Sdk, NAPS2.Lib.Common, NAPS2.Lib.WinForms)
}