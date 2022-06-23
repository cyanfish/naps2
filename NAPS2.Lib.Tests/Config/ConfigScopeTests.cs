using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class ConfigScopeTests : ContextualTexts
{
    [Fact]
    public void InternalDefaultsNotNullProps()
    {
        var config = InternalDefaults.GetCommonConfig();
        AssertPropNullOrNotNull(config, false, "");
    }

    private static void AssertPropNullOrNotNull(object config, bool shouldBeNull, string path)
    {
        Assert.True(config != null, path);
        foreach (var prop in config.GetType().GetProperties())
        {
            var value = prop.GetValue(config);
            if (prop.CustomAttributes.Any(x => typeof(ChildAttribute).IsAssignableFrom(x.AttributeType)))
            {
                // Child, so recurse
                AssertPropNullOrNotNull(value, shouldBeNull, $"{path}{prop.Name}.");
            }
            else
            {
                if (shouldBeNull)
                {
                    Assert.True(value == null, $"{prop.DeclaringType?.Name}.{prop.Name} == null");
                }
                else
                {
                    Assert.True(value != null, $"{prop.DeclaringType?.Name}.{prop.Name} != null");
                }
            }
        }
    }

    [Fact]
    public void FileScope()
    {
        var configPath = Path.Combine(FolderPath, "config.xml");
        var scope = new FileConfigScope<CommonConfig>(configPath, new ConfigSerializer(ConfigReadMode.All), ConfigScopeMode.ReadWrite);
            
        // Nothing should be created yet
        Assert.False(File.Exists(configPath));

        // Reading should get the default value
        Assert.False(scope.TryGet(c => c.Culture, out _));

        // Writing should save to the file
        scope.Set(c => c.Culture, "fr");
        Assert.True(File.Exists(configPath));
        var doc = XDocument.Load(configPath);
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