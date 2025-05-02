using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools.Droid;

public static class Aapt
{
    private static readonly Regex _xmlTreeNamespaceRegex = new(@"^N:\s*(?<ns>[^=]+)=(?<url>.*)$");
    private static readonly Regex _xmlTreeElementRegex = new(@"^E:\s*((?<ns>[^:]+):)?(?<name>.*) \(line=\d+\)$");
    private static readonly Regex _xmlTreeAttributeRegex = new(@"^A:\s*((?<ns>[^:]+):)?(?<name>[^(]+)(\(.*\))?=(?<value>.*)$");

    private static string[] AaptPaths => AndroidAaptPathProvider.GetPaths();

    public static AndroidManifest GetAndroidManifest(string apkFilePath)
    {
        foreach (var aaptPath in AaptPaths)
            try
            {
                var result = ExternalProgramRunner.Run(
                    aaptPath,
                    $"dump xmltree \"{apkFilePath}\" AndroidManifest.xml",
                    log: false,
                    message: $"Loading AndroidManifest.xml using {aaptPath}");

                return new AndroidManifest(ParseXmlTree(result.Output));
            }
            catch (Exception e)
            {
                Log.Write($"Warning: Failed to load Android Manifest.xml using {aaptPath}: {e.Message}");
            }

        throw new Exception($"Failed to load Android Manifest.xml using {apkFilePath}");
    }

    private static XDocument ParseXmlTree(string xmltree)
    {
        var separators = new[] { "\n", "\r\n" };
        var lines = xmltree.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        var xDocument = new XDocument();

        var stack = new Stack<ParsedElement>();
        stack.Push(new ParsedElement(xDocument, 0));

        var namespaces = new Dictionary<string, XNamespace>();

        foreach (var line in lines)
            ParseXmlTreeLine(line, stack, namespaces);

        return xDocument;
    }

    private static void ParseXmlTreeLine(string line, Stack<ParsedElement> stack, Dictionary<string, XNamespace> namespaces)
    {
        var trimmedLine = line.TrimStart();
        var indent = line.Length - trimmedLine.Length;

        if (trimmedLine.StartsWith('N'))
        {
            var match = _xmlTreeNamespaceRegex.Match(trimmedLine);
            if (!match.Success)
                throw new Exception($"Invalid namespace: {line}");

            var namespaceName = match.Groups["ns"].Value;
            if (!namespaces.ContainsKey(namespaceName))
                namespaces.Add(namespaceName, XNamespace.Get(match.Groups["url"].Value));
        }
        else if (trimmedLine.StartsWith('E'))
        {
            // pop out if the current line is higher than previous
            while (stack.Count > 0 && stack.Peek().Indent >= indent)
                stack.Pop();

            var match = _xmlTreeElementRegex.Match(trimmedLine);
            if (!match.Success)
                throw new Exception($"Invalid element: {line}");

            var element = new XElement(GetXName(match, namespaces, line));

            // this is the first element, so add the namespaces to it
            if (stack.Count == 1)
                foreach (var pair in namespaces)
                    element.Add(new XAttribute(XNamespace.Xmlns + pair.Key, pair.Value));

            stack.Peek().Container.Add(element);
            stack.Push(new ParsedElement(element, indent));
        }
        else if (trimmedLine.StartsWith('A'))
        {
            var match = _xmlTreeAttributeRegex.Match(trimmedLine);
            if (!match.Success)
                throw new Exception($"Invalid attribute: {line}");

            // TODO: parse the (type) and use the correct value

            var value = match.Groups["value"].Value;
            var strMatch = Regex.Match(value, """\"(?<value>.*)\"\s*\(Raw:.*\)""");
            var xName = GetXName(match, namespaces, line);
            stack.Peek()
                .Container.Add(
                    strMatch.Success
                        ? new XAttribute(xName, strMatch.Groups["value"].Value)
                        : new XAttribute(xName, value));
        }
    }

    private static XName GetXName(Match match, Dictionary<string, XNamespace> namespaces, string line)
    {
        var namespaceName = match.Groups["ns"].Value;
        if (!string.IsNullOrWhiteSpace(namespaceName) && !namespaces.ContainsKey(namespaceName))
            throw new Exception($"Unknown xml namespace: {namespaceName}.");

        XName xName;
        try
        {
            xName = string.IsNullOrWhiteSpace(namespaceName)
                ? XName.Get(match.Groups["name"].Value)
                : XName.Get(match.Groups["name"].Value, namespaces[namespaceName].ToString());
        }
        catch
        {
            throw new Exception($"Invalid attribute: {line}");
        }

        return xName;
    }

    private class ParsedElement
    {
        public ParsedElement(XContainer container, int indent)
        {
            Container = container;
            Indent = indent;
        }

        public XContainer Container { get; }

        public int Indent { get; }
    }
}