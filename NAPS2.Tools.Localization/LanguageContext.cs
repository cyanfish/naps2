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
            using (var reader = new StreamReader(poFile))
            {
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

            UpdateProjectFile(file.FullName, savePath);
        }

        private void UpdateProjectFile(string dependentPath, string savePath)
        {
            string relativePath = RelativePath(savePath);
            string relativeDependentPath = RelativePath(dependentPath);
            string projectFilePath = Path.Combine(Paths.Root, @"NAPS2.Core\NAPS2.Core.csproj");
            var doc = XDocument.Load(projectFilePath);
            var schema = "http://schemas.microsoft.com/developer/msbuild/2003";

            var oldElement = doc.Descendants(XName.Get("EmbeddedResource", schema)).FirstOrDefault(x => x.Attribute("Include")?.Value == relativePath);
            if (oldElement != null)
            {
                // Comment this and uncomment below if conventions change and the project file needs to be updated from scratch
                return;
            }
            // oldElement?.Remove();

            var dependentElement = doc.Descendants(XName.Get("EmbeddedResource", schema)).FirstOrDefault(x => x.Attribute("Include")?.Value == relativeDependentPath);
            var doubleDependent = dependentElement?.Element(XName.Get("DependentUpon", schema));
            if (doubleDependent != null)
            {
                relativeDependentPath = doubleDependent.Value;
            }

            var siblings = doc.Descendants(XName.Get("EmbeddedResource", schema)).Where(x => SameDependentFile(x.Attribute("Include")?.Value, relativePath)).OrderBy(x => x.Attribute("Include")?.Value).ToList();
            int index;
            for (index = 0; index < siblings.Count; index++)
            {
                if (StringComparer.Ordinal.Compare(siblings[index].Attribute("Include")?.Value, relativePath) >= 0)
                {
                    break;
                }
            }

            if (siblings.Count > 0)
            {
                var newElement = new XElement(XName.Get("EmbeddedResource", schema));
                newElement.SetAttributeValue("Include", relativePath);
                newElement.SetAttributeValue("Condition", "'$(Configuration)' != 'Debug'");
                newElement.Add(new XElement(XName.Get("DependentUpon", schema), Path.GetFileName(relativeDependentPath)));
                if (index == 0)
                {
                    siblings[0].AddBeforeSelf(newElement);
                }
                else
                {
                    siblings[index - 1].AddAfterSelf(newElement);
                }
            }

            doc.Save(projectFilePath);
        }

        private static string RelativePath(string savePath)
        {
            return savePath.Substring(savePath.LastIndexOf(@"NAPS2.Core\", StringComparison.Ordinal) + 11);
        }

        private static bool SameDependentFile(string path1, string path2)
        {
            var name1 = Path.GetFileName(path1);
            var name2 = Path.GetFileName(path2);
            return Path.GetDirectoryName(path1) == Path.GetDirectoryName(path2) &&
                   name1?.Substring(0, name1.IndexOf('.')) == name2?.Substring(0, name2.IndexOf('.'));
        }
    }
}