namespace NAPS2.Tools.Localization;

public class LanguageContext
{
    private readonly string _langCode;

    public Dictionary<string, TranslatableString> Strings { get; } = new Dictionary<string, TranslatableString>();

    public LanguageContext(string langCode)
    {
        _langCode = langCode;
    }

    public void Load(string poFile)
    {
        using var reader = new StreamReader(poFile);
        string? line;
        string? NextLine() => reader.ReadLine()?.Trim();
        while ((line = NextLine()) != null)
        {
            if (!line.StartsWith("msgid", StringComparison.InvariantCulture))
            {
                continue;
            }

            string original = line.Substring(7, line.Length - 8);
            while ((line = NextLine()) != null && line.StartsWith("\"", StringComparison.InvariantCulture))
            {
                original += line.Substring(1, line.Length - 2);
            }

            if (line == null || !line.StartsWith("msgstr", StringComparison.InvariantCulture))
            {
                continue;
            }

            string translated = line.Substring(8, line.Length - 9);
            while ((line = NextLine()) != null && line.StartsWith("\"", StringComparison.InvariantCulture))
            {
                translated += line.Substring(1, line.Length - 2);
            }

            Strings[original] = new TranslatableString(original, translated);
        }
    }

    public void Translate(string folder, bool winforms)
    {
        foreach (var file in new DirectoryInfo(folder).GetFiles("*.resx"))
        {
            if (file.Name.Count(x => x == '.') == 1)
            {
                TranslateFile(file, winforms);
            }
        }
    }

    private void TranslateFile(FileInfo file, bool winforms)
    {
        var doc = XDocument.Load(file.FullName);
        bool hasSomethingTranslatable = false;

        foreach (var item in doc.Root!.Elements("data").ToList())
        {
            var prop = item.Attribute("name")?.Value;
            var original = item.Element("value")?.Value;
            if (prop == null || original == null || !Rules.IsTranslatable(winforms, prop, ref original, out string prefix, out string suffix))
            {
                // Trim untranslatable strings from localized resx files
                item.Remove();
                continue;
            }
            hasSomethingTranslatable = true;
            if (Strings.ContainsKey(original))
            {
                var translation = Strings[original].Translation;
                if (!string.IsNullOrWhiteSpace(translation))
                {
                    item.Element("value")!.Value = prefix + Strings[original].Translation + suffix;
                }
            }
        }

        if (!hasSomethingTranslatable) return;

        // Trim redundant elements from localized resx files
        doc.DescendantNodes().OfType<XComment>().Remove();
        doc.Descendants(XName.Get("schema", "http://www.w3.org/2001/XMLSchema")).Remove();
        doc.Descendants("metadata").Remove();
        doc.Descendants("assembly").Remove();

        string savePath = file.FullName.Replace(".resx", $".{_langCode}.resx");
        doc.Save(savePath);
    }
}