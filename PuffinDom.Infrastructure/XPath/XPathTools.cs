using System.Xml;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;
using Wmhelp.XPath2;

namespace PuffinDom.Infrastructure.XPath;

public class XPathTools
{
    public static List<ViewData> EvaluateXPath(Platform platform, string viewName, string xPath, string xml)
    {
        try
        {
            if (xPath.IsEmpty())
                throw new ArgumentException("XPath is empty");

            Log.Write($"{viewName} XPath: {xPath}");

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            return xmlDocument
                .XPath2SelectNodes(xPath)
                .Cast<XmlNode>()
                .Select(xmlNode => new ViewData(xmlNode, platform))
                .ToList();
        }
        catch (Exception e)
        {
            Log.Write(e, $"Error while selecting nodes for {viewName}");
            throw new TechnicalCrashFailTestException($"Error while selecting nodes for {viewName}");
        }
    }
}