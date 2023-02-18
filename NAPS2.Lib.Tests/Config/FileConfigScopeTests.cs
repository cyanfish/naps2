using System.Threading;
using NAPS2.Config.Model;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class FileConfigScopeTests : ContextualTests
{
    // TODO: Split up into different tests
    [Fact]
    public void FileScope()
    {
        var configPath = Path.Combine(FolderPath, "config.xml");
        var scope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.All, ConfigRootName.UserConfig), ConfigScopeMode.ReadWrite);
            
        // Nothing should be created yet
        Assert.False(File.Exists(configPath));

        // Reading should get the default value
        Assert.False(scope.TryGet(c => c.Culture, out _));

        // Writing should save to the file
        scope.Set(c => c.Culture, "fr");
        Assert.True(File.Exists(configPath));

        var doc = XDocument.Load(configPath);
        Assert.Equal("UserConfig", doc.Root?.Name);
        
        var docValue = doc.Descendants("Culture").Single().Value;
        Assert.Equal("fr", docValue);

        // Reading should read back the value
        Assert.Equal("fr", scope.GetOrDefault(c => c.Culture));

        // Setting a different value shouldn't affect the first
        scope.Set(c => c.CheckForUpdates, true);
        Assert.True(scope.GetOrDefault(c => c.CheckForUpdates));
        Assert.Equal("fr", scope.GetOrDefault(c => c.Culture));

        // Lock the file
        using (var stream = new FileStream(configPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // If changes can't be persisted, they should still be saved in memory
            scope.Set(c => c.Culture, "de");
            scope.Set(c => c.ThumbnailSize, 64);
            Assert.Equal("de", scope.GetOrDefault(c => c.Culture));
            Assert.Equal(64, scope.GetOrDefault(c => c.ThumbnailSize));
            Assert.True(scope.GetOrDefault(c => c.CheckForUpdates));

            // Now directly modify the file
            Assert.False(scope.TryGet(c => c.DisableAutoSave, out _));
            DirectSetValue(stream, "DisableAutoSave", "true");
            Assert.False(scope.TryGet(c => c.DisableAutoSave, out _));
        }

        Thread.Sleep(500);
        // Now that the file is unlocked, it should read the correct value
        Assert.True(scope.GetOrDefault(c => c.DisableAutoSave));

        // Lock again
        using (var stream = new FileStream(configPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // Reading should not persisted the changes yet
            doc = XDocument.Load(stream);
            docValue = doc.Descendants("Culture").Single().Value;
            Assert.Equal("fr", docValue);
            Assert.Equal("de", scope.GetOrDefault(c => c.Culture));

            // Directly modify the file again
            DirectSetValue(stream, "ComponentsPath", "test_path");
        }

        // Now saving anything should persist all our changes without affecting the concurrent changes
        scope.Set(c => c.NoUserProfiles, false);
        doc = XDocument.Load(configPath);
        Assert.Equal("de", doc.Descendants("Culture").Single().Value);
        Assert.Equal("64", doc.Descendants("ThumbnailSize").Single().Value);
        Assert.Equal("true", doc.Descendants("DisableAutoSave").Single().Value);
        Assert.Equal("test_path", doc.Descendants("ComponentsPath").Single().Value);
        Assert.Equal("false", doc.Descendants("NoUserProfiles").Single().Value);
    }

    [Fact]
    public void ReadWithBadXml()
    {
        var configPath = Path.Combine(FolderPath, "config.xml");
        File.WriteAllText(configPath, @"blah");
        var scope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.All, ConfigRootName.UserConfig), ConfigScopeMode.ReadWrite);

        Assert.False(scope.TryGet(c => c.Culture, out _));
    }

    [Fact]
    public void ReadWithBadConfig()
    {
        var configPath = Path.Combine(FolderPath, "config.xml");
        File.WriteAllText(configPath, @"<?xml version=""1.0"" encoding=""utf-8""?><Blah><Culture>fr</Culture></Blah>");
        var scope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.DefaultOnly, ConfigRootName.AppConfig), ConfigScopeMode.ReadWrite);

        Assert.False(scope.TryGet(c => c.Culture, out _));
    }

    [Fact]
    public void ReadWithMissingFile()
    {
        var configPath = Path.Combine(FolderPath, "config.xml");
        var scope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.DefaultOnly, ConfigRootName.AppConfig), ConfigScopeMode.ReadWrite);

        Assert.False(scope.TryGet(c => c.Culture, out _));
    }

    [Fact]
    public void ReadAppSettings()
    {
        var configPath = Path.Combine(FolderPath, "appsettings.xml");
        File.WriteAllText(configPath, ConfigData.AppSettings);
        var defaultsScope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.DefaultOnly, ConfigRootName.AppConfig), ConfigScopeMode.ReadOnly);
        var lockedScope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.LockedOnly, ConfigRootName.AppConfig), ConfigScopeMode.ReadOnly);

        Assert.False(lockedScope.TryGet(c => c.AlwaysRememberDevice, out _));
        Assert.True(lockedScope.TryGet(c => c.PdfSettings.Compat, out var pdfCompat));
        Assert.Equal(PdfCompat.PdfA1B, pdfCompat);

        Assert.False(defaultsScope.TryGet(c => c.PdfSettings.Compat, out _));
        Assert.True(defaultsScope.TryGet(c => c.AlwaysRememberDevice, out var alwaysRememberDevice));
        Assert.True(alwaysRememberDevice);
    }

    [Fact]
    public void ReadWithOldConfig()
    {
        var configPath = Path.Combine(FolderPath, "config.xml");
        File.WriteAllText(configPath, ConfigData.OldUserConfig);
        var scope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.All, ConfigRootName.UserConfig), ConfigScopeMode.ReadWrite);

        Assert.False(scope.TryGet(c => c.LockSystemProfiles, out _));
        Assert.True(scope.TryGet(c => c.OcrMode, out var ocrMode));
        Assert.Equal(LocalizedOcrMode.Best, ocrMode);
    }

    private static void DirectSetValue(FileStream stream, string tagName, string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        stream.Seek(0, SeekOrigin.Begin);
        var doc = XDocument.Load(stream);
        var tag = doc.Descendants(tagName).SingleOrDefault();
        if (tag == null)
        {
            tag = new XElement(tagName);
            doc.Root!.Add(tag);
        }
        tag.RemoveAttributes(); // Remove xsi:nil
        tag.Value = value;
        stream.Seek(0, SeekOrigin.Begin);
        doc.Save(stream);
        stream.SetLength(stream.Position);
    }
}