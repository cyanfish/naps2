using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NAPS2.Localization
{
    public class TemplatesContext
    {
        private const string HEADER = @"msgid """"
msgstr """"
""Project-Id-Version: PACKAGE VERSION\n""
""Report-Msgid-Bugs-To: \n""
""POT-Creation-Date: 2016-04-19 21:51+0000\n""
""PO-Revision-Date: YEAR-MO-DA HO:MI+ZONE\n""
""Last-Translator: FULL NAME <EMAIL@ADDRESS>\n""
""Language-Team: LANGUAGE <LL@li.org>\n""
""Language: sk\n""
""MIME-Version: 1.0\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Content-Transfer-Encoding: 8bit\n""
""X-Generator: Translate Toolkit 1.13.0\n""
""X-Poedit-SourceCharset: iso-8859-1\n""";

        public Dictionary<string, TranslatableString> Strings { get; } = new Dictionary<string, TranslatableString>();

        public void Load(string folder, bool winforms)
        {
            foreach (var file in new DirectoryInfo(folder).GetFiles("*.resx"))
            {
                if (file.Name.Count(x => x == '.') == 1)
                {
                    LoadFile(file, winforms);
                }
            }
        }

        private void LoadFile(FileInfo file, bool winforms)
        {
            var doc = XDocument.Load(file.FullName);
            foreach (var item in doc.Root.Elements("data"))
            {
                var prop = item.Attribute("name")?.Value;
                var original = item.Element("value")?.Value;
                if (!Rules.IsTranslatable(winforms, prop, ref original, out _, out _))
                {
                    continue;
                }
                if (!Strings.ContainsKey(original))
                {
                    Strings[original] = new TranslatableString { Original = original, Context = new List<string>()};
                }
                Strings[original].Context.Add($"{file.Name}${prop}$Message");
            }
        }

        public void Save(string path)
        {
            using (var writer = new StreamWriter(path))
            {
                writer.Write(HEADER);
                writer.Write("\r\n\r\n");
                foreach (var str in Strings.Values.OrderBy(x => x.Original, StringComparer.Ordinal))
                {
                    foreach (var context in str.Context.OrderBy(x => x))
                    {
                        writer.Write($"#: {context}\r\n");
                    }
                    writer.Write($"msgid \"{str.Original.Replace("\"", "\\\"")}\"\r\n");
                    writer.Write($"msgstr \"\"\r\n");
                    writer.Write("\r\n");
                }
            }
        }
    }
}