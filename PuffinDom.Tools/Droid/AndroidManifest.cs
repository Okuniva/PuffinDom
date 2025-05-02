using System;
using System.Linq;
using System.Xml.Linq;

namespace PuffinDom.Tools.Droid;

public class AndroidManifest
{
    private static readonly XNamespace _xmlnsAndroid = "http://schemas.android.com/apk/res/android";

    public AndroidManifest(XDocument xDocument)
    {
        Document = xDocument ?? throw new ArgumentNullException(nameof(xDocument));
    }

    public XDocument Document { get; }

    public string PackageName =>
        Document.Root?.Attribute("package")?.Value!;

    
    public string MainLauncherActivity =>
        Document.Root?
            .Element("application")?
            .Elements("activity")?
            .FirstOrDefault(
                a =>
                    a.Element("intent-filter")
                        ?.Element("action")
                        ?.Attribute(_xmlnsAndroid + "name")
                        ?.Value == "android.intent.action.MAIN" &&
                    a.Element("intent-filter")
                        ?.Element("category")
                        ?.Attribute(_xmlnsAndroid + "name")
                        ?.Value == "android.intent.category.LAUNCHER")?
            .Attribute(_xmlnsAndroid + "name")?
            .Value!;
}