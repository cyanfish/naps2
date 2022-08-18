using System.Xml.Linq;

namespace NAPS2.Escl;

public static class EsclXmlHelper
{
    public static readonly XNamespace ScanNs = XNamespace.Get("http://schemas.hp.com/imaging/escl/2011/05/03");
    public static readonly XNamespace PwgNs = XNamespace.Get("http://www.pwg.org/schemas/2010/12/sm");

    private static readonly XDeclaration Decl = new("1.0", "UTF-8", null);
    private static readonly XAttribute ScanNsAttr = new(XNamespace.Xmlns + "scan", ScanNs);
    private static readonly XAttribute PwgNsAttr = new(XNamespace.Xmlns + "pwg", PwgNs);

    public static string CreateDocAsString(XElement root)
    {
        root.Add(ScanNsAttr);
        root.Add(PwgNsAttr);
        var content = new XDocument(root);
        return $"{Decl}{Environment.NewLine}{content}";
    }
}