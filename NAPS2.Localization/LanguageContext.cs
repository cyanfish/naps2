using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            using (var reader = new StreamReader(poFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("msgid"))
                    {
                        var original = line.Substring(7, line.Length - 8);
                        line = reader.ReadLine();
                        if (line == null || !line.StartsWith("msgstr"))
                        {
                            continue;
                        }
                        var translated = line.Substring(8, line.Length - 9);
                        Strings[original] = new TranslatableString
                        {
                            Original = original,
                            Translation = translated
                        };
                    }
                }
            }
        }

        public void Translate(string folder)
        {
            foreach (var file in new DirectoryInfo(folder).GetFiles("*.resx"))
            {
                if (file.Name.Count(x => x == '.') == 1)
                {
                    TranslateFile(file);
                }
            }
        }

        private void TranslateFile(FileInfo file)
        {
            var doc = XDocument.Load(file.FullName, LoadOptions.PreserveWhitespace);
            foreach (var item in doc.Root.Elements("data"))
            {
                var name = item.Attribute("name")?.Value;
                var value = item.Element("value")?.Value;
                if (name == null || value == null || !value.Any(char.IsLetter))
                {
                    continue;
                }
                var match = Regex.Match(value, @"[:.]+$");
                value = Regex.Replace(value, @"[:.]+$", "");
                if (Strings.ContainsKey(value))
                {
                    item.Element("value").Value = Strings[value].Translation + match.Value;
                }
            }

            doc.Save(file.FullName.Replace(".resx", $".{langCode}.resx"), SaveOptions.DisableFormatting);
        }
    }
}