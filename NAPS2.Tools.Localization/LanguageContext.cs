using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NAPS2.Localization
{
    public class LanguageContext
    {
        private readonly string langCode;

        public Dictionary<string, TranslatableString> Strings { get; } = new Dictionary<string, TranslatableString>();

        public LanguageContext(string langCode)
        {
            this.langCode = langCode;
        }

        public void Load(string poFile)
        {
            using var reader = new StreamReader(poFile);
            string line;
            string NextLine() => line = reader.ReadLine()?.Trim();
            while (NextLine() != null)
            {
                if (!line.StartsWith("msgid", StringComparison.InvariantCulture))
                {
                    continue;
                }

                string original = line.Substring(7, line.Length - 8);
                while (NextLine() != null && line.StartsWith("\"", StringComparison.InvariantCulture))
                {
                    original += line.Substring(1, line.Length - 2);
                }

                if (line == null || !line.StartsWith("msgstr", StringComparison.InvariantCulture))
                {
                    continue;
                }

                string translated = line.Substring(8, line.Length - 9);
                while (NextLine() != null && line.StartsWith("\"", StringComparison.InvariantCulture))
                {
                    translated += line.Substring(1, line.Length - 2);
                }

                Strings[original] = new TranslatableString
                {
                    Original = original,
                    Translation = translated
                };
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

            foreach (var item in doc.Root.Elements("data").ToList())
            {
                var prop = item.Attribute("name")?.Value;
                var original = item.Element("value")?.Value;
                if (!Rules.IsTranslatable(winforms, prop, ref original, out string prefix, out string suffix))
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
                        item.Element("value").Value = prefix + Strings[original].Translation + suffix;
                    }
                }
            }

            if (!hasSomethingTranslatable) return;

            // Trim redundant elements from localized resx files
            doc.DescendantNodes().OfType<XComment>().Remove();
            doc.Descendants(XName.Get("schema", "http://www.w3.org/2001/XMLSchema")).Remove();
            doc.Descendants("metadata").Remove();
            doc.Descendants("assembly").Remove();

            string savePath = file.FullName.Replace(".resx", $".{langCode}.resx");
            doc.Save(savePath);
        }
    }
}