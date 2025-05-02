using System.Reflection;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.XPath;
using PuffinDom.Tools;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Scroll;
using PuffinDom.UI.Views;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Extensions;

public static class ViewExtensions
{
    private const int DefaultScrollsTries = 7;

    public static bool DoesNotExist(this View view)
    {
        return InnerViewExistsValidation(view, false);
    }

    public static bool Exists(this View view)
    {
        return InnerViewExistsValidation(view, true);
    }

    private static bool InnerViewExistsValidation(this View view, bool checkExists)
    {
        using var logContext = Log.PushContext(
            $"Checking {view} " +
            $"{(checkExists ? "exists" : "does not exist")} now");

        var results = view.EvaluateQuery("Exists validation");
        var exists = results.Any();

        Log.Write(
            exists
                ? $"{view} exists"
                : $"{view} does not exist");

        return checkExists
            ? exists
            : !exists;
    }

    public static TView DoubleTap<TView>(this TView view)
        where TView : View
    {
        using var logContext = Log.PushContext($"Double tapping {view}");

        UIContext.Device.TapCoordinates(
            view.Rect.CenterX,
            view.Rect.CenterY,
            2);

        ThreadSleep.For(
            PuffinConstants.DefaultDelayAfterAnyAction,
            "Delay after double tap");

        return view;
    }

    public static TView ScrollDownTo<TView>(
        this View view,
        TView viewToFind,
        int maxScrolls = 5,
        bool assert = true,
        int? scrollDistance = null,
        string? message = null,
        Query? queryToGetElementsToCheckIfSomethingChangedAfterScroll = null)
        where TView : View =>
        ViewScrollingStrategy.ScrollTo(
            view,
            viewToFind,
            maxScrolls,
            ScrollingType.Vertical,
            ScrollingStrategy.Down,
            assert,
            scrollDistance,
            message,
            queryToGetElementsToCheckIfSomethingChangedAfterScroll);

    public static TView ScrollTo<TView>(
        this View view,
        TView viewToFind,
        int maxScrolls = 5,
        ScrollingType scrollingType = ScrollingType.Vertical,
        ScrollingStrategy? scrollingStrategy = null,
        bool assert = true,
        int? scrollDistance = null,
        string? message = null,
        Query? queryToGetElementsToCheckIfSomethingChangedAfterScroll = null)
        where TView : View =>
        ViewScrollingStrategy.ScrollTo(
            view,
            viewToFind,
            maxScrolls,
            scrollingType,
            scrollingStrategy,
            assert,
            scrollDistance,
            message,
            queryToGetElementsToCheckIfSomethingChangedAfterScroll);

    public static TView TryToScrollAroundAndCheckThereIsNoView<TView, TChildView>(
        this TView view,
        TChildView viewToCheck,
        int maxScrolls = DefaultScrollsTries,
        bool scrollTopThenBottom = true)
        where TView : View
        where TChildView : View
    {
        using var logContext = Log.PushContext(
            $"Waiting NO row {viewToCheck} in {view} with maxScrolls = {maxScrolls}, scrollTopThenBottom = {scrollTopThenBottom}");

        if (viewToCheck.Exists())
            throw new ScrollException($"Row {viewToCheck} exists");

        var scrollOrderInvolved = view
            .GetType()
            .GetProperties()
            .Any(x => x.GetCustomAttribute<ScrollOrderAttribute>() != null);

        if (scrollOrderInvolved)
        {
            try
            {
                ScrollTo(view, viewToCheck);
            }
            catch (ViewNotFoundException)
            {
                return view;
            }

            throw new ScrollException($"Row {viewToCheck} has no scroll order");
        }

        if (scrollTopThenBottom)
            for (var i = 0; i < maxScrolls; i++)
            {
                view.Drag(Direction.Up);

                if (viewToCheck.Exists())
                    throw new ScrollException($"Row {viewToCheck} exists");
            }

        var count = 0;
        while (count < maxScrolls)
        {
            view.Drag(Direction.Down);
            count++;

            if (viewToCheck.Exists())
                throw new ScrollException($"Row {viewToCheck} exists");
        }

        return view;
    }

    public static bool TryToScrollAroundAndCheckIsViewWithWaitFalseExist<TView, TChildView>(
        this TView view,
        TChildView viewToCheck,
        int maxScrolls = DefaultScrollsTries,
        bool scrollTopThenBottom = true)
        where TView : View
        where TChildView : View
    {
        using var logContext = Log.PushContext(
            $"Trying to find row {viewToCheck} in {view} with maxScrolls = {maxScrolls}, scrollTopThenBottom = {scrollTopThenBottom}");

        if (viewToCheck.Exists())
            return true;

        var scrollOrderInvolved = view
            .GetType()
            .GetProperties()
            .Any(x => x.GetCustomAttribute<ScrollOrderAttribute>() != null);

        if (scrollOrderInvolved)
            try
            {
                ScrollTo(view, viewToCheck);
                return true;
            }
            catch (ViewNotFoundException)
            {
                return false;
            }

        if (scrollTopThenBottom)
            for (var i = 0; i < maxScrolls; i++)
            {
                view.Drag(Direction.Up);

                if (viewToCheck.Exists())
                    return true;
            }

        for (var i = 0; i < maxScrolls; i++)
        {
            view.Drag(Direction.Down);

            if (viewToCheck.Exists())
                return true;
        }

        return false;
    }

    public static TView SwipeRight<TView>(this TView view)
        where TView : View
    {
        return view.Drag(Direction.Right);
    }

    public static TView SwipeLeft<TView>(this TView view)
        where TView : View
    {
        return view.Drag(Direction.Left);
    }

    public static TView Drag<TView>(
        this TView view,
        Direction direction,
        int? distanceToDragPx = null,
        TimeSpan? duration = null,
        int? startYPxPosition = null)
        where TView : View
    {
        using var logContext = Log.PushContext($"Dragging {view} to {direction}");

        var rect = view is IRootView
            ? UIContext.Device.DeviceRect
            : view.Rect;

        switch (direction)
        {
            case Direction.Up:
            {
                distanceToDragPx = CalculateDistanceToDragInPx(
                    distanceToDragPx,
                    rect);

                var fromYPx = startYPxPosition ?? rect.CenterY - rect.Height / 10;
                var upToYPx = fromYPx + distanceToDragPx.Value;

                if (upToYPx > UIContext.Device.DeviceRect.Height || upToYPx < 0)
                    upToYPx = UIContext.Device.DeviceRect.Height - 10.ConvertFromDpToPx();

                Log.Write($"Dragging ↑ {fromYPx} to {upToYPx} for {Math.Abs(fromYPx - upToYPx)} px");

                UIContext.Device.DragCoordinates(
                    rect.CenterX,
                    fromYPx,
                    rect.CenterX,
                    upToYPx,
                    duration: duration);

                ThreadSleep.For(
                    PuffinConstants.DelayAfterScrollDuration,
                    "Delay after scrolling up");

                break;
            }
            case Direction.Down:
            {
                distanceToDragPx = CalculateDistanceToDragInPx(
                    distanceToDragPx,
                    rect);

                var fromYPx = startYPxPosition ?? rect.CenterY + rect.Height / 10;
                var downToYPx = fromYPx - distanceToDragPx.Value;

                if (downToYPx < 0 || downToYPx > UIContext.Device.DeviceRect.Height)
                    downToYPx = 20.ConvertFromDpToPx();

                Log.Write($"Dragging ↓ {fromYPx} to {downToYPx} for {Math.Abs(fromYPx - downToYPx)} px");

                UIContext.Device.DragCoordinates(
                    rect.CenterX,
                    fromYPx,
                    rect.CenterX,
                    downToYPx,
                    duration: duration);

                ThreadSleep.For(
                    PuffinConstants.DelayAfterScrollDuration,
                    "Delay after scrolling down");

                break;
            }
            case Direction.Left:
            {
                var fromX = rect.CenterX + rect.Width / 10;

                var toX = distanceToDragPx == null
                    ? rect.X + rect.Width / 100
                    : fromX + distanceToDragPx.Value;

                Log.Write($"Dragging ← {fromX} to {toX} for {Math.Abs(fromX - toX)} px");

                UIContext.Device.DragCoordinates(
                    fromX,
                    rect.CenterY,
                    toX,
                    rect.CenterY,
                    duration: duration);

                ThreadSleep.For(
                    PuffinConstants.DelayAfterSwipeDuration,
                    "Delay after swiping left");

                break;
            }
            case Direction.Right:
            {
                var fromX = rect.CenterX - rect.Width / 10;
                var toX = distanceToDragPx == null
                    ? rect.X + rect.Width - rect.Width / 100
                    : rect.X - distanceToDragPx.Value;

                Log.Write($"Dragging → {fromX} to {toX} for {Math.Abs(fromX - toX)} px");

                UIContext.Device.DragCoordinates(
                    fromX,
                    rect.CenterY,
                    toX,
                    rect.CenterY,
                    duration: duration);

                ThreadSleep.For(
                    PuffinConstants.DelayAfterSwipeDuration,
                    "Delay after swiping right");

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        return view;
    }

    private static int CalculateDistanceToDragInPx(int? distanceToDrag, Rect rect)
    {
        distanceToDrag ??= (int)(rect.Height / 1.4);

        if (distanceToDrag.Value >= PuffinConstants.MinimumDpToScrollUpOrDown.ConvertFromDpToPx())
            return distanceToDrag.Value;

        Log.Write(
            distanceToDrag.Value <= 0
                ? $"WARNING! Distance to drag is negative! {distanceToDrag.Value} px. Setting distance to default"
                : $"WARNING! Too small distance is dragging {distanceToDrag.Value} px. Setting distance to default");

        distanceToDrag = PuffinConstants.MinimumDpToScrollUpOrDown.ConvertFromDpToPx();

        return distanceToDrag.Value;
    }

    public static TVew WaitForAnimationsStops<TVew>(this TVew view)
        where TVew : View
    {
        using var logContext = Log.PushContext("Waiting for animations stops");

        var currentX = view.Rect.CenterX;
        var currentY = view.Rect.CenterY;

        UIContext.Device.InvalidateCachedPageSource();

        ViewWaitingStrategy.WaitCondition(
            () =>
            {
                if (view.Rect.CenterX == currentX && view.Rect.CenterY == currentY)
                    return true;

                currentX = view.Rect.CenterX;
                currentY = view.Rect.CenterY;
                return false;
            },
            "Animations stops",
            true);

        return view;
    }

    public static TView TapAndHold<TView>(this TView view, bool checkScreenClosed = false)
        where TView : View
    {
        using var logContext = Log.PushContext($"Touching and holding center of {view}: [{view.Rect.CenterX}:{view.Rect.CenterY}]");

        ThreadSleep.For(1.Second(), "Workaround TapAndHold doesn't work. Try to delete");

        UIContext.Device.TouchAndHoldRect(view.Rect);

        ThreadSleep.For(PuffinConstants.DefaultDelayAfterAnyAction, "Delay after TapAndHold action");

        if (!checkScreenClosed)
            return view;

        view.ValidateRootParentDisappeared();

        return view;
    }

    public static TView DragAndDropToTheMiddleOfScreen<TView>(this TView view, bool dragFromRightToLeft = true)
        where TView : View
    {
        using var logContext = Log.PushContext(
            $"Dragging and dropping to the middle of screen {view} with dragFromRightToLeft = {dragFromRightToLeft}");

        var screenSize = UIContext.Device.DeviceRect.Width;
        var centerX = (int)Math.Round(screenSize / 2d);

        var rect = view.Rect;
        var x = dragFromRightToLeft ? rect.Width : rect.X;
        var y = rect.Y;

        UIContext.Device.DragCoordinates(x, y, centerX, y);

        ThreadSleep.For(
            PuffinConstants.DelayAfterSwipeDuration,
            "Delay after dragging to the middle of screen");

        return view;
    }

    public static bool ScrollDownToAvoidOverlayViews<TView>(
        this TView scrollableArea,
        View viewAbove,
        View viewBelow)
        where TView : View
    {
        using var logContext = Log.PushContext($"Scrolling down to avoid overlay views {viewAbove} and {viewBelow}");

        var viewAboveHeight = viewAbove.Rect.Height;
        var viewAboveY = viewAbove.Rect.Y;
        var viewBelowHeight = viewBelow.Rect.Height;
        var viewBelowY = viewBelow.Rect.Y;

        if (IsBelowViewNotFullyOverAboveView())
        {
            scrollableArea.Drag(Direction.Down, viewBelowY - viewAboveY + viewBelowHeight);
            return true;
        }

        return false;

        bool IsBelowViewNotFullyOverAboveView()
            => viewAboveY < viewBelowY && viewAboveY + viewAboveHeight > viewBelowY;
    }

    public static List<ViewData> EvaluateQuery(this View view, string? customMessage = null)
    {
        return EvaluateXPath(
            view.ViewFullName,
            view.FullQuery(""),
            customMessage,
            view.GetFullPageSource);
    }

    public static List<ViewData> EvaluateXPath(
        string viewName,
        string xPath,
        string? customMessage = null,
        bool full = false)
    {
        return XPathTools.EvaluateXPath(
            UIContext.Platform,
            viewName,
            xPath,
            UIContext.Device.GetScreenAsXml(
                customMessage == null
                    ? viewName
                    : $"{viewName} | {customMessage}",
                full));
    }

    public static TView Wait<TView>(this TView view, TimeSpan? timeOut = null)
        where TView : View<TView>
    {
        view.WaitSucceeded(true, timeOut);
        return view;
    }

    public static View Wait(this View view, TimeSpan? timeOut = null)
    {
        view.WaitSucceeded(true, timeOut);
        return view;
    }

    public static View TryWait<TView>(this TView view, TimeSpan? timeOut = null)
        where TView : View<TView>
    {
        view.WaitSucceeded(false, timeOut);
        return view;
    }
}