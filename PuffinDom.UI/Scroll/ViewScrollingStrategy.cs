using JetBrains.Annotations;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.XPath;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Scroll;

public class ViewScrollingStrategy
{
    public static TView ScrollTo<TView>(
        View view,
        TView viewToFind,
        int maxScrolls = 5,
        ScrollingType scrollingType = ScrollingType.Vertical,
        ScrollingStrategy? scrollingStrategy = null,
        bool assert = false,
        int? scrollDistance = null,
        string? message = null,
        Func<string, string>? queryToGetElementsToCheckIfSomethingChangedAfterScroll = null)
        where TView : View
    {
        return ScrollTo(
            view,
            viewToFind,
            out _,
            maxScrolls,
            scrollingType,
            scrollingStrategy,
            assert,
            scrollDistance,
            message,
            queryToGetElementsToCheckIfSomethingChangedAfterScroll);
    }

    [PublicAPI]
    public static TView ScrollTo<TView>(
        View view,
        TView viewToFind,
        out ScrollHistory scrollHistory,
        int maxScrolls = 5,
        ScrollingType scrollingType = ScrollingType.Vertical,
        ScrollingStrategy? scrollingStrategy = null,
        bool assert = false,
        int? scrollDistance = null,
        string? message = null,
        Func<string, string>? queryToGetElementsToCheckIfSomethingChangedAfterScroll = null)
        where TView : View
    {
        Log.Write($"Scrolling to {viewToFind} in {view}");

        scrollHistory = ScrollTo(
            view,
            SearchPredicate,
            maxScrolls,
            scrollingType,
            scrollingStrategy,
            false,
            scrollDistance,
            message,
            queryToGetElementsToCheckIfSomethingChangedAfterScroll);

        return !SearchPredicate() && assert
            ? throw new ViewNotFoundException(
                $"{viewToFind} is not on screen but should be there after {scrollHistory.BackMovements} ↑ and {scrollHistory.ForwardMovements} ↓ moves")
            : viewToFind;

        bool SearchPredicate() => viewToFind.Exists();
    }

    public static ScrollHistory ScrollTo(
        View view,
        Func<bool> predicate,
        int maxScrolls = 5,
        ScrollingType scrollingType = ScrollingType.Vertical,
        ScrollingStrategy? scrollingStrategy = null,
        bool assert = false,
        int? scrollDistance = null,
        string? message = null,
        Func<string, string>? queryToGetElementsToCheckIfSomethingChangedAfterScroll = null,
        int? startYPxPosition = null)
    {
        if (UIContext.iOS)
            maxScrolls += 5;

        switch (scrollingType)
        {
            default:
            case ScrollingType.Vertical:
                scrollingStrategy ??= ScrollingStrategy.UpThenDown;
                break;
            case ScrollingType.Horizontal:
                scrollingStrategy ??= ScrollingStrategy.LeftThenRight;
                break;
        }

        using var logContext = Log.PushContext(
            $"Scrolling in {view} " +
            $"with maxScrolls = {maxScrolls}, " +
            $"scrollingStrategy = {scrollingStrategy}, " +
            $"scrollingType = {scrollingType} " +
            $"assert = {assert} " +
            $"scrollDistance = {scrollDistance} " +
            $"message = {message}" +
            $"queryToGetElementsToCheckIfSomethingChangedAfterScroll = {queryToGetElementsToCheckIfSomethingChangedAfterScroll}");

        var scrollHistory = new ScrollHistory();

        if (predicate())
            return scrollHistory;

        var needsMoreScrolls = true;

        if ((scrollingStrategy == ScrollingStrategy.UpThenDown && scrollingType == ScrollingType.Vertical) ||
            (scrollingStrategy == ScrollingStrategy.LeftThenRight && scrollingType == ScrollingType.Horizontal))
            for (var i = 0; i < maxScrolls && needsMoreScrolls; i++)
            {
                var preResult =
                    CollectScrollNecessityResult(queryToGetElementsToCheckIfSomethingChangedAfterScroll);

                view.Drag(
                    scrollingType == ScrollingType.Vertical
                        ? Direction.Up
                        : Direction.Left,
                    scrollDistance,
                    startYPxPosition: startYPxPosition);

                scrollHistory.BackMovements++;

                if (predicate())
                    return scrollHistory;

                needsMoreScrolls
                    = CalculateIfMoreScrollsNeeded(
                        queryToGetElementsToCheckIfSomethingChangedAfterScroll,
                        preResult,
                        needsMoreScrolls);
            }

        var count = 0;
        needsMoreScrolls = true;
        while (count < maxScrolls && needsMoreScrolls)
        {
            var preResult =
                CollectScrollNecessityResult(
                    queryToGetElementsToCheckIfSomethingChangedAfterScroll);

            view.Drag(
                scrollingType == ScrollingType.Vertical
                    ? Direction.Down
                    : Direction.Right,
                scrollDistance,
                startYPxPosition: startYPxPosition);

            scrollHistory.ForwardMovements++;

            count++;

            if (predicate())
                return scrollHistory;

            needsMoreScrolls
                = CalculateIfMoreScrollsNeeded(
                    queryToGetElementsToCheckIfSomethingChangedAfterScroll,
                    preResult,
                    needsMoreScrolls);
        }

        if (!assert)
            return scrollHistory;

        const string baseMessage = "Predicate scrolling not successful";
        var finalMessage = message == null
            ? baseMessage
            : message + " | " + baseMessage;

        throw new ViewNotFoundException(finalMessage);
    }

    private static bool CalculateIfMoreScrollsNeeded(
        Func<string, string>? queryToGetElementsToCheckIfSomethingChangedAfterScroll,
        List<ViewData>? preResult,
        bool needsMoreScrolls)
    {
        using var screenshotsContext = UIContext.Device.TurnOffScreenshots();

        if (preResult != null)
        {
            var resultAfter = CollectScrollNecessityResult(
                queryToGetElementsToCheckIfSomethingChangedAfterScroll);

            if (preResult.Count > 1
                && resultAfter.Count > 1
                && preResult.Count == resultAfter.Count
                && CompareLists(preResult, resultAfter))
                needsMoreScrolls = false;
        }

        Log.Write(
            needsMoreScrolls
                ? "Needs more scrolls"
                : "No need for more scrolls");

        return needsMoreScrolls;
    }

    private static List<ViewData> CollectScrollNecessityResult(
        Func<string, string>? queryToGetElementsToCheckIfSomethingChangedAfterScroll)
    {
        List<ViewData>? result;

        using var screenshotsContext = UIContext.Device.TurnOffScreenshots();

        if (queryToGetElementsToCheckIfSomethingChangedAfterScroll != null)
            result = XPathTools.EvaluateXPath(
                UIContext.Platform,
                "Specific predefined views filter for scroll scenario",
                queryToGetElementsToCheckIfSomethingChangedAfterScroll(""),
                UIContext.Device.GetScreenAsXml("Scrolling Calculations"));
        else
            result = UIContext.Platform switch
            {
                Platform.Android => XPathTools.EvaluateXPath(
                    UIContext.Platform,
                    "Specific predefined views filter for scroll scenario",
                    "//*[@displayed='true']",
                    UIContext.Device.GetScreenAsXml("Scrolling Calculations")),
                Platform.iOS => XPathTools.EvaluateXPath(
                    UIContext.Platform,
                    "Specific predefined views filter for scroll scenario",
                    "//*[@enabled='true']",
                    UIContext.Device.GetScreenAsXml("Scrolling Calculations")),
                _ => [],
            };

        return result;
    }

    private static bool CompareLists(List<ViewData> list1, List<ViewData> list2)
    {
        return list1.Zip(list2, (first, second) => first.Equals(second)).All(equal => equal);
    }
}