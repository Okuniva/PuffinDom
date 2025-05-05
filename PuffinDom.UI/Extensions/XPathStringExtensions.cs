using JetBrains.Annotations;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.UI.Extensions;

public static class XPathStringExtensions
{
    public static string RootElementName => UIContext.Platform switch
    {
        Platform.Android => "hierarchy",
        Platform.iOS => ClosestChildren("AppiumAUT"),
        _ => throw new ArgumentOutOfRangeException()
    };

    public static string ScreenWithTabsLineView<TTabType>(
        this string query,
        string tabsLineViewId,
        Func<TTabType, string> tabToTitleConverter,
        params TTabType[] tabsToCheck)
        where TTabType : struct, Enum
    {
        var values = tabsToCheck.IsEmpty()
            ? Enum.GetValues<TTabType>()
            : tabsToCheck;

        var tabs = values
            .Select(tabToTitleConverter)
            .Select(
                x => UIContext.Android && UIContext.DeviceApiVersion != "21"
                    ? x.ToUpper()
                    : x)
            .ToArray();

        var result = query;

        for (var index = 0; index < tabs.Length - 1; index++)
            result = query
                .Id(tabsLineViewId)
                .Text(tabs[index])
                .RootParent();

        return result
            .Id(tabsLineViewId)
            .Text(tabs.Last());
    }

    public static string RootParent(this string query)
    {
        return query.Parent(RootElementName);
    }

    [PublicAPI]
    public static string AllChildren(this string query)
    {
        return $"{query}//*";
    }

    public static string ElementAndAllChildren(this string query)
    {
        return $"({query}) | ({AllChildren(query)})";
    }

    [PublicAPI]
    public static string Parent(this string query, string parentClass)
    {
        return $"{query}/ancestor::{parentClass}";
    }

    public static string IdAndTextAndSelected(this string query, string id, string text)
    {
        return $"{query}//*[{PlainId(id)} and @selected='true' and {PlainText(text)}]";
    }

    public static string ListOfItemsWithId(this string query, string childId)
    {
        return query + ClassWithChildWithId("android.widget.ListView", childId);
    }

    public static string ElementWithClassAndChildrenWithTexts(this string query, string @class, string childText, params string[] allTexts)
    {
        var result = $"{query}//*[{PlainClass(@class)} and .//*[{PlainText(childText)}]";

        foreach (var anotherChildText in allTexts)
            result += $" and .//*[{PlainText(anotherChildText)}]";

        return $"{result}]";
    }

    [PublicAPI]
    public static string ElementWithClassAndNoChildWithText(this string query, string @class, string childText)
    {
        return $"{query}//*[{PlainClass(@class)} and not(.//*[{PlainText(childText)}])]";
    }

    public static string ElementWithClassAndChildWithId(this string query, string @class, string childId)
    {
        return $"{query}//*[{PlainClass(@class)} and .//*[{PlainId(childId)}]]";
    }

    [PublicAPI]
    public static string ElementWithClassAndChildrenWithIds(this string query, string @class, string childId, params string[] allIds)
    {
        var result = $"{query}//*[{PlainClass(@class)} and .//*[{PlainId(childId)}]";

        foreach (var alternativeId in allIds)
            result += $" and .//*[{PlainId(alternativeId)}]";

        return $"{result}]";
    }

    public static string ElementWithClassAndClosestChildWithId(this string query, string @class, string childId)
    {
        return $"{query}//{@class}[.//*[{PlainId(childId)}]]";
    }

    public static string ElementWithIdWithChildrenWithIdsAndOneText(this string query, string id, string text, params string[] idsToCheck)
    {
        var result = $"{query}//*[{PlainId(id)} and ";

        for (var index = 0; index < idsToCheck.Length - 1; index++)
        {
            var childId = idsToCheck[index];
            result += $".//*[{PlainId(childId)}] and ";
        }

        return $"{result} .//*[{PlainText(text)}]]";
    }

    public static string ElementWithIdWithChildrenWithIds(this string query, string id, params string[] idsToCheck)
    {
        var result = $"{query}//*[{PlainId(id)} and ";

        for (var index = 0; index < idsToCheck.Length - 1; index++)
        {
            var childId = idsToCheck[index];
            result += $".//*[{PlainId(childId)}] and ";
        }

        return $"{result} .//*[{PlainId(idsToCheck.Last())}]]";
    }

    public static string Dialog(this string query, string textToCheck, params string[] otherTextsToCheck)
    {
        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return query.ElementWithChildrenWithTexts(textToCheck, otherTextsToCheck);
            case Platform.iOS:
                return query.Class("XCUIElementTypeAlert")
                    .ElementWithChildrenWithTexts(textToCheck, otherTextsToCheck)
                    .Parent("XCUIElementTypeAlert");
        }
    }

    public static string ElementWithChildrenWithTexts(this string query, string textToCheck, params string[] otherTextsToCheck)
    {
        var result = query
            .Text(textToCheck);

        if (otherTextsToCheck.Any())
            result = result.RootParent();

        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var index = 0; index < otherTextsToCheck.Length; index++)
        {
            var text = otherTextsToCheck[index];
            result = result.Text(text);

            if (index != otherTextsToCheck.Length - 1)
                result = result.RootParent();
        }

        return result;
    }

    private static string ClassWithChildWithId(string @class, string childId)
    {
        return $"//*[{PlainClass(@class)} and .//*[{PlainId(childId)}]]";
    }

    private static string PlainClass(string @class)
    {
        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return $"@class='{@class}'";
            case Platform.iOS:
                return $"@type='{@class}'";
        }
    }

    private static string PlainId(string id, string? packageId = null)
    {
        packageId ??= $"{UIContext.PackageId}:id/";

        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return $"@resource-id='{packageId}{id}'";
            case Platform.iOS:
                return $"@name='{id}'";
        }
    }

    public static string Id(this string query, string id)
    {
        return $"{query}//*[{PlainId(id)}]";
    }

    public static string IdOrAlternativeIds(this string query, string id, params string[] alternativeIds)
    {
        var result = $"{query}//*[{PlainId(id)}";

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var alternativeId in alternativeIds)
            result += $" or {PlainId(alternativeId)}";

        return result + "]";
    }

    public static string TextAndIdOrAlternativeIds(this string query, string text, string id, params string[] alternativeIds)
    {
        var result = $"{query}//*[{PlainId(id)}";

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var alternativeId in alternativeIds)
            result += $" or {PlainId(alternativeId)}";

        return result + $"and {PlainText(text)}]";
    }

    public static string IdOrClass(this string query, string id, string className)
    {
        return $"{query}//*[{PlainId(id)} or {PlainClass(className)}]";
    }

    public static string ClosestChildren(this string query)
    {
        return $"{query}/*";
    }

    public static string ClosestChildWithClass(this string query, string className)
    {
        return $"{query}/*[{PlainClass(className)}]";
    }

    public static string IdForSystemApp(this string query, string id)
    {
        return $"{query}//*[{PlainId(id, "")}]";
    }

    public static string IdAndTextForSystemApp(this string query, string id, string text)
    {
        return $"{query}//*[{PlainId(id, "")} and {PlainText(text)}]";
    }

    public static string ClosestParentOfChildWithId(this string query, string id)
    {
        return query + $"//*[{PlainId(id)}]/..";
    }

    public static string IdAndText(this string query, string id, string text)
    {
        return $"{query}//*[{PlainId(id)} and {PlainText(text)}]";
    }

    public static string IdAndValue(this string query, string id, string value)
    {
        return $"{query}//*[{PlainId(id)} and @value='{value}']";
    }

    public static string Class(this string query, string className, params string[] alternativeClassNames)
    {
        var result = $"{query}//*[{PlainClass(className)}";

        foreach (var alternativeClassName in alternativeClassNames)
            result += $" or {PlainClass(alternativeClassName)}";

        return result + "]";
    }

    public static string IosClassAndSkipWithNames(this string query, string className, string[] skipWithNames)
    {
        var result = $"{query}//*[{PlainClass(className)}";
        foreach (var name in skipWithNames)
            result += $" and @name!='{name}'";

        return result + "]";
    }

    public static string IdAndTextContains(this string query, string id, string text)
    {
        return $"{query}//*[{PlainId(id)} and {PlainContains(text)}]";
    }

    public static string ContentDescription(this string query, string marked)
    {
        return $"{query}//*[@content-desc='{marked}']";
    }

    public static string ClassAndText(this string query, string className, string text)
    {
        return $"{query}//*[{PlainClass(className)} and {PlainText(text)}]";
    }

    public static string ClassAndTextContains(this string query, string className, string text)
    {
        return $"{query}//*[{PlainClass(className)} and {PlainContains(text)}]";
    }

    public static string Text(this string query, string text, params string[] alternativeTexts)
    {
        var result = $"{query}//*[{PlainText(text)}";

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var alternativeText in alternativeTexts)
            result += $" or {PlainText(alternativeText)}";

        result += "]";

        return result;
    }

    private static string PlainText(string text)
    {
        var safeText = MakeTextSafe(text);

        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return $"@text={safeText}";
            case Platform.iOS:
                return $"@label={safeText}";
        }
    }

    private static string MakeTextSafe(string text)
    {
        var safeText = text.Replace("&", "&amp;");
        if (safeText.Contains('\''))
            safeText = "concat('" + safeText.Replace("'", "', \"'\", '") + "')";
        else
            safeText = $"'{safeText}'";

        return safeText;
    }

    [Obsolete("Find proper alternative")]
    public static string Sibling(this string query)
    {
        return $"{query}/following-sibling::*";
    }

    public static string ElementWithIdAndChildrenWithTexts(this string query, string id, string text, params string[] allTexts)
    {
        var result = $"{query}//*[{PlainId(id)} and .//*[{PlainText(text)}]";

        foreach (var anotherChildText in allTexts)
            result += $" and .//*[{PlainText(anotherChildText)}]";

        return result + "]";
    }

    public static string ElementWithIdAndChildWhichContainsText(this string query, string id, string text)
    {
        return $"{query}//*[{PlainId(id)} and .//*[{PlainContains(text)}]]";
    }

    private static string PlainContains(string text)
    {
        var safeText = MakeTextSafe(text);

        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return $"contains(@text, {safeText})";
            case Platform.iOS:
                return $"contains(@label, {safeText})";
        }
    }

    public static string TextInputClass(this string query)
    {
        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return query.Class("android.widget.EditText");
            case Platform.iOS:
                return query.Class("UIFieldEditor", "XCUIElementTypeTextField", "XCUIElementTypeSearchField", "XCUIElementTypeSecureTextField");
        }
    }

    public static string IOSTableCellClass(this string query)
    {
        return query.Class("XCUIElementTypeCell");
    }

    public static string IOSTableCellClassWithoutElementWithText(this string query, string text)
    {
        return query.ElementWithClassAndNoChildWithText("XCUIElementTypeCell", text);
    }

    public static string IOSTableCellWithChildrenWithIds(this string query, string id, params string[] allIds)
    {
        return query.ElementWithClassAndChildrenWithIds("XCUIElementTypeCell", id, allIds);
    }

    public static string IOSTableCellWithChildrenWithTexts(this string query, string text, params string[] allTexts)
    {
        return query.ElementWithClassAndChildrenWithTexts("XCUIElementTypeCell", text, allTexts);
    }

    public static string CheckBoxClass(this string query)
    {
        return UIContext.Android
            ? $"{query}//*[{PlainClass("android.widget.CheckBox")}]"
            : $"{query}//*[{PlainClass("XCUIElementTypeSwitch")}]";
    }

    public static string TextLabelClass(this string query)
    {
        return $"{query}//*[{PlainClass("android.widget.TextView")}]";
    }

    public static string ListViewClass(this string query)
    {
        return $"{query}//*[{PlainClass("android.widget.ListView")}]";
    }

    public static string ButtonClass(this string query)
    {
        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return Class(query, "android.widget.Button");
            case Platform.iOS:
                return Class(query, "XCUIElementTypeButton");
        }
    }

    public static string Index(this string query, int index, bool wrapExpression = false)
    {
        index = AdjustToXPathNotation(index);

        return wrapExpression
            ? $"({query})[{index}]"
            : $"{query}[{index}]";

        int AdjustToXPathNotation(int cSharpIndex)
        {
            return cSharpIndex + 1;
        }
    }
}