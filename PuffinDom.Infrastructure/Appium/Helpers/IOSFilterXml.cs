using System;
using System.Linq;
using System.Xml.Linq;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Infrastructure.Appium.Helpers;

public class IOSFilterXml
{
    private readonly IDeviceManager _deviceManager;

    public IOSFilterXml(IDeviceManager deviceManager)
    {
        _deviceManager = deviceManager;
    }

    public string FilterXml(string xml)
    {
        using var logContext = Log.PushContext("iOS Filter XML");

        var document = XDocument.Parse(xml);

        AddTypeAttributeWithNameOfTheNode(document.Root!);

        LeaveOnlyVisibleNodes(document.Root!);

        return document.ToString();
    }

    public static string FilterFullPageXml(string xml)
    {
        using var logContext = Log.PushContext("iOS Filter Full Page XML");

        var document = XDocument.Parse(xml);

        CutAllInvisibleNodes(document.Root!);
        return document.ToString();
    }

    private static void AddTypeAttributeWithNameOfTheNode(XElement documentRoot)
    {
        foreach (var element in documentRoot.Descendants())
            element.Add(new XAttribute("type", element.Name.LocalName));
    }

    private static void CutAllInvisibleNodes(XElement elements)
    {
        foreach (var child in elements.Elements().ToList())
        {
            var vis = child.Attribute("visible")?.Value;

            if (bool.TryParse(vis, out var visible))
                if (!visible)
                {
                    child.Remove();
                    continue;
                }

            CutAllInvisibleNodes(child);
        }
    }

    private void LeaveOnlyVisibleNodes(XElement element, bool elementHasScrollableParent = false, Rect? scrollableParentViewRect = null)
    {
        foreach (var child in element.Elements().ToList())
        {
            var xAttr = child.Attribute("x")?.Value;
            var yAttr = child.Attribute("y")?.Value;
            var widthAttr = child.Attribute("width")?.Value;
            var heightAttr = child.Attribute("height")?.Value;
            var type = child.Attribute("type")?.Value;

            string? parentY = null;
            string? parentX = null;
            if (child.Parent is { IsEmpty: false } && child.Parent.Attribute("y") != null && child.Parent.Attribute("x") != null)
            {
                parentY = child.Parent.Attribute("y")!.Value;
                parentX = child.Parent.Attribute("x")!.Value;
            }

            if (int.TryParse(xAttr, out var x) &&
                int.TryParse(yAttr, out var y) &&
                int.TryParse(widthAttr, out var width) &&
                int.TryParse(heightAttr, out var height))
            {
                var maxWidth = _deviceManager.DeviceRect.Width;
                var maxHeight = _deviceManager.DeviceRect.Height;

                var isOutFromScrollView = false;
                if (elementHasScrollableParent && scrollableParentViewRect != null)
                {
                    isOutFromScrollView = x + width <= scrollableParentViewRect.X || y + height <= scrollableParentViewRect.Y;
                    var scrollableInnerMaxHeight = scrollableParentViewRect.Height + scrollableParentViewRect.Y;
                    maxHeight = Math.Min(scrollableInnerMaxHeight, _deviceManager.DeviceRect.Height);
                    maxWidth = scrollableParentViewRect.Width + scrollableParentViewRect.X;
                }

                switch (type)
                {
                    case "XCUIElementTypeCell":
                        LeaveOnlyVisibleNodes(child, true, new Rect(x, y, width, height));
                        break;
                    case "XCUIElementTypeScrollView" or "XCUIElementTypeTable":
                        scrollableParentViewRect = new Rect(x, y, width, height);
                        break;
                }

                var isContainer = height > maxHeight || width > maxWidth;
                var isXOut = x >= maxWidth;
                var isYOut = y >= maxHeight;

                var isViewOut = isYOut || isXOut || isOutFromScrollView;

                if (!isContainer && isViewOut)
                {
                    child.Remove();
                    continue;
                }

                if (isViewOut)
                    continue;

                if (parentY != null && parentX != null)
                {
                    if (y < int.Parse(parentY))
                        child.SetAttributeValue("y", parentY);

                    if (x < int.Parse(parentX))
                        child.SetAttributeValue("x", parentX);
                }

                if (x + width > maxWidth)
                    child.SetAttributeValue("width", (maxWidth - x).ToString());

                if (y + height > maxHeight)
                    child.SetAttributeValue("height", (maxHeight - y).ToString());
            }

            LeaveOnlyVisibleNodes(
                child,
                elementHasScrollableParent || type == "XCUIElementTypeScrollView" || type == "XCUIElementTypeTable",
                scrollableParentViewRect);
        }
    }
}