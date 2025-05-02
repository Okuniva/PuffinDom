using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools.IOS;


public class PList
{
    private readonly string _plistPath;

    public XDocument? Xdoc;

    public PList(string plistPath)
    {
        _plistPath = plistPath ?? throw new ArgumentNullException(nameof(plistPath));
    }

    public XDocument GetDocument()
    {
        if (Xdoc != null)
            return Xdoc;

        Log.Write($"Loading PList {_plistPath}...");

        try
        {
            using var stream = File.OpenRead(_plistPath);
            Xdoc = XDocument.Load(stream, LoadOptions.None);

            return Xdoc;
        }
        catch (XmlException)
        {
            Log.Write("Unable to load PList as XML, trying a decoded version...");

            var result = ExternalProgramRunner.Run(
                "plutil",
                $"-convert xml1 -o - \"{_plistPath}\"",
                message: $"Decoding PList {_plistPath}");

            using var reader = new StringReader(result.Output);
            Xdoc = XDocument.Load(reader, LoadOptions.None);

            return Xdoc;
        }
    }

    public string? GetStringValue(string key)
    {
        var xDocument = GetDocument();

        var keyElements = xDocument.Descendants("key").Where(x => x.Value == key).ToArray();

        switch (keyElements.Length)
        {
            case 0:
            case < 1:
                return null;
            case > 1:
                throw new Exception($"Found multiple instances of key {key}.");
            case 1:
                var value = keyElements[0].ElementsAfterSelf().FirstOrDefault();
                if (value == null)
                    throw new Exception($"Unable to find value for key {key}.");

                return value.Value;
        }
    }

    public string? GetBundleIdentifier() =>
        GetStringValue("CFBundleIdentifier");
}