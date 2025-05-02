using System;
using System.Xml;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Logging;
using OpenQA.Selenium.Appium;

namespace PuffinDom.Infrastructure.Appium;

public sealed class ViewData
{
    public ViewData(XmlNode xmlElement, Platform platform)
    {
        try
        {
            Class = platform == Platform.Android
                ? xmlElement.Attributes!["class"]!.Value
                : xmlElement.Attributes!["type"]!.Value;

            switch (platform)
            {
                case Platform.Android when Class == "hierarchy":
                {
                    var width = xmlElement.Attributes!["width"]!.Value;
                    var height = xmlElement.Attributes!["height"]!.Value;

                    Rect = new Rect(0, 0, int.Parse(width), int.Parse(height));
                    break;
                }
                case Platform.Android:
                {
                    var boundsString = xmlElement.Attributes!["bounds"]!.Value;

                    var bounds = boundsString.Substring(1, boundsString.Length - 2).Split("][");
                    var point1 = bounds[0].Split(",");
                    var point2 = bounds[1].Split(",");

                    Rect = new Rect(
                        int.Parse(point1[0]),
                        int.Parse(point1[1]),
                        int.Parse(point2[0]) - int.Parse(point1[0]),
                        int.Parse(point2[1]) - int.Parse(point1[1])
                    );

                    break;
                }
                case Platform.iOS:
                {
                    var x = xmlElement.Attributes!["x"]!.Value;
                    var y = xmlElement.Attributes!["y"]!.Value;
                    var width = xmlElement.Attributes!["width"]!.Value;
                    var height = xmlElement.Attributes!["height"]!.Value;

                    Rect = new Rect(
                        int.Parse(x),
                        int.Parse(y),
                        int.Parse(width),
                        int.Parse(height)
                    );

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(Platform), platform, "Unsupported platform");
            }

            if (platform == Platform.Android)
                Text = xmlElement.Attributes["text"]?.Value ?? string.Empty;
            else
            {
                if (Class is "XCUIElementTypeTextField" or "XCUIElementTypeSecureTextField" or "XCUIElementTypeSearchField")
                    Text = xmlElement.Attributes["value"]?.Value ?? string.Empty;
                else if (xmlElement.Attributes["label"] != null)
                    Text = xmlElement.Attributes["label"]?.Value ?? string.Empty;
                else
                    Text = xmlElement.Attributes["value"]?.Value ?? string.Empty;
            }

            if (platform == Platform.Android)
            {
                var value = xmlElement.Attributes["resource-id"]?.Value ?? string.Empty;
                IdForLogs = value;
            }
            else
                IdForLogs = xmlElement.Attributes["name"]?.Value ?? string.Empty;

            Selected = xmlElement.Attributes["selected"] != null
                       && bool.Parse(xmlElement.Attributes["selected"]!.Value);

            if (Class == "XCUIElementTypeButton")
            {
                var value = xmlElement.Attributes["value"]?.Value;
                if (value != null)
                    Selected = true;
            }

            Enabled = xmlElement.Attributes["enabled"] != null && bool.Parse(xmlElement.Attributes["enabled"]!.Value);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (platform)
            {
                case Platform.Android:
                    Checked = xmlElement.Attributes["checked"] != null && bool.Parse(xmlElement.Attributes["checked"]!.Value);
                    break;
                case Platform.iOS:
                {
                    if (Class == "XCUIElementTypeSwitch")
                    {
                        var value = xmlElement.Attributes["value"]?.Value;
                        var valueAsInt = int.Parse(value!);
                        Checked = valueAsInt == 1;
                    }

                    break;
                }
            }
        }
        catch (Exception e)
        {
            Log.Write(e, "Failed to parse element:");
            Log.Write(xmlElement.OuterXml);
            throw;
        }
    }

    
    public ViewData(AppiumElement appiumElement, string idForLogs)
    {
        ArgumentNullException.ThrowIfNull(appiumElement);

        try
        {
            var boundsString = appiumElement.Rect;

            Rect = new Rect(
                boundsString.X,
                boundsString.Y,
                boundsString.Width,
                boundsString.Height
            );

            Text = appiumElement.Text;

            IdForLogs = idForLogs;
        }
        catch (Exception e)
        {
            Log.Write(e, $"Failed to parse element with id: {idForLogs}");
            throw new Exception("Failed to parse element", e);
        }
    }

    public bool Checked { get; }

    public bool Selected { get; }

    public Rect Rect { get; }
    public bool Enabled { get; }
    public string Text { get; }
    public string IdForLogs { get; }
    public string? Class { get; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ViewData);
    }

    public bool Equals(ViewData? element2)
    {
        if (element2 is null)
            return false;

        const int tolerance = 5;

        return IdForLogs == element2.IdForLogs
               && Text == element2.Text
               && Checked == element2.Checked
               && Class == element2.Class
               && Enabled == element2.Enabled
               && Math.Abs(Rect.X - element2.Rect.X) <= tolerance
               && Math.Abs(Rect.Y - element2.Rect.Y) <= tolerance
               && Math.Abs(Rect.Height - element2.Rect.Height) <= tolerance
               && Math.Abs(Rect.Width - element2.Rect.Width) <= tolerance
               && Selected == element2.Selected;
    }

    public override int GetHashCode()
    {
        var hash = 17;

        hash = hash * 23 + IdForLogs.GetHashCode();
        hash = hash * 23 + Text.GetHashCode();
        hash = hash * 23 + Checked.GetHashCode();
        hash = hash * 23 + (Class?.GetHashCode() ?? 0);
        hash = hash * 23 + Enabled.GetHashCode();
        hash = hash * 23 + Rect.X.GetHashCode();
        hash = hash * 23 + Rect.Y.GetHashCode();
        hash = hash * 23 + Rect.Height.GetHashCode();
        hash = hash * 23 + Rect.Width.GetHashCode();
        hash = hash * 23 + Selected.GetHashCode();

        return hash;
    }
}