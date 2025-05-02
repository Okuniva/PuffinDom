using System.Reflection;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Scroll;

public class ScrollOrderViewHelper
{
    public static bool ScrollToViewWithScrollableIndex(View view, string viewName)
    {
        if (view.Parent == null || !PropertyHasAttribute(view.Parent, viewName))
            return false;

        if (ScrollOrderAttribute.OrderReceivingInProgress)
            return true;

        if (view.Exists())
            return true;

        ScrollToView(view, viewName);

        return true;
    }

    private static void ScrollToView(View view, string viewName)
    {
        if (view.Parent == null)
            throw new TechnicalCrashFailTestException("ScrollToView: Parent is null");

        var currentBlocksOrder = GetViewsOrder(view.Parent);
        var currentIndex = GetScrollableIndex(view.Parent, viewName);

        var parentToCurrentParentContainsChildrenWithScrollOrderAttribute =
            view.Parent.Parent?
                .GetType()
                .GetProperties()
                .Any(x => x.GetCustomAttribute<ScrollOrderAttribute>() != null) ?? false;

        var parentToScrollOn = CalculateViewToScroll(
            view,
            parentToCurrentParentContainsChildrenWithScrollOrderAttribute);

        if (currentBlocksOrder[0] > currentIndex)
            parentToScrollOn.ScrollTo(
                view,
                scrollingStrategy: ScrollingStrategy.UpThenDown);
        else if (currentBlocksOrder.Last() < currentIndex)
            parentToScrollOn.ScrollTo(
                view,
                scrollingStrategy: ScrollingStrategy.Down);
        else
            view.AssertExists($"{view} should be {currentIndex} but not found. Currently visible are: {string.Join(", ", currentBlocksOrder)}");
    }

    private static View CalculateViewToScroll(View view, bool parentToCurrentParentContainsChildrenWithScrollOrderAttribute)
    {
        return parentToCurrentParentContainsChildrenWithScrollOrderAttribute
            ? view.Parent.NotNull().Parent.NotNull()
            : view.Parent.NotNull();
    }

    private static bool PropertyHasAttribute(View parent, string viewName)
    {
        return parent
            .GetType()
            .GetProperty(viewName)
            ?.GetCustomAttribute<ScrollOrderAttribute>() != null;
    }

    private static List<int> GetViewsOrder<TEnum>(Func<TEnum, View> getView)
        where TEnum : struct, Enum
    {
        List<int> viewsOrderList = [];
        ViewWaitingStrategy.WaitCondition(
            () =>
            {
                using (Log.GetDangerousDisposableNoLogsContext("Getting page state to calculate scroll position"))
                {
                    using (Log.GetDangerousDisposableNoLogsContext("Getting page state to calculate scroll position"))
                    {
                        viewsOrderList = Enum.GetValues<TEnum>()
                            .Select(
                                x => new Tuple<TEnum, int>(
                                    x,
                                    typeof(TEnum).GetMember(x.ToString())[0].GetCustomAttribute<ScrollOrderAttribute>()!.Index))
                            .OrderBy(x => x.Item2)
                            .Select(
                                x =>
                                {
                                    try
                                    {
                                        using var viewWaitingWithScrollingContext = ScrollOrderAttribute.TurnOnDangerousCurrentViewOrderRegime();

                                        var view = getView(x.Item1);
                                        return view.Exists()
                                            ? x
                                            : null;
                                    }
                                    catch (Exception)
                                    {
                                        return null;
                                    }
                                })
                            .Where(x => x != null)
                            .Select(x => x!.Item2)
                            .ToList();

                        return viewsOrderList.Any();
                    }
                }
            },
            "Views order receiving");

        ValidateViewsOrderIsCorrect(viewsOrderList, typeof(TEnum).ToString());

        return viewsOrderList;
    }

    private static void ValidateViewsOrderIsCorrect(List<int> blocksOrderList, string viewName)
    {
        if (!blocksOrderList.Any())
            throw new ViewNotFoundException($"No views were found. {viewName} or it's closest parent isn't on screen");

        for (var i = 1; i < blocksOrderList.Count; i++)
            if (blocksOrderList[i] < blocksOrderList[i - 1])
                throw new Exception($"Block order is strange: {blocksOrderList[i]} < {blocksOrderList[i - 1]}");
    }

    private static int GetScrollableIndex(View parent, string viewName)
    {
        var property = parent
            .GetType()
            .GetProperties()
            .FirstOrDefault(x => x.Name == viewName);

        if (property == null)
            throw new TechnicalCrashFailTestException($"Property not found. All the views must have {nameof(ScrollOrderAttribute)}");

        return property
            .GetCustomAttribute<ScrollOrderAttribute>()
            .NotNull(nameof(ScrollOrderAttribute))
            .Index;
    }

    private static List<int> GetViewsOrder(View parent)
    {
        List<int> viewsOrderList = [];
        ViewWaitingStrategy.WaitCondition(
            () =>
            {
                using (Log.GetDangerousDisposableNoLogsContext("Getting page state to calculate scroll position"))
                {
                    using (ScrollOrderAttribute.TurnOnDangerousCurrentViewOrderRegime())
                    {
                        viewsOrderList = parent
                            .GetType()
                            .GetProperties()
                            .Where(
                                x =>
                                {
                                    var scrollOrderAttribute = x.GetCustomAttribute<ScrollOrderAttribute>() != null
                                        ? x.GetCustomAttribute<ScrollOrderAttribute>()
                                            .NotNull()
                                        : null;

                                    if (scrollOrderAttribute == null)
                                        return false;

                                    if (UIContext.Android)
                                        return scrollOrderAttribute.ScrollOrderElementType != ScrollOrderElementType.Fixed
                                               && scrollOrderAttribute.ScrollOrderElementType !=
                                               ScrollOrderElementType.FixedOnAndroid_NotFixedOnIOS;

                                    return scrollOrderAttribute.ScrollOrderElementType != ScrollOrderElementType.Fixed;
                                })
                            .Select(
                                x => new Tuple<PropertyInfo, int>(
                                    x,
                                    x.GetCustomAttribute<ScrollOrderAttribute>()!.Index))
                            .OrderBy(x => x.Item2)
                            .Select(
                                x =>
                                {
                                    try
                                    {
                                        var localView = x.Item1.GetValue(parent)!.CastTo<View>();
                                        var result = localView.Exists()
                                            ? x
                                            : null;

                                        return result;
                                    }
                                    catch (Exception)
                                    {
                                        return null;
                                    }
                                })
                            .Where(x => x != null)
                            .Select(x => x!.Item2)
                            .ToList();

                        return viewsOrderList.Any();
                    }
                }
            },
            "Views order receiving");

        ValidateViewsOrderIsCorrect(viewsOrderList, parent.ViewFullName);

        return viewsOrderList;
    }

    public static View ScrollToEnumValueWithScrollableIndex<TEnum>(
        View parent,
        TEnum enumValue,
        Func<TEnum, View> getView,
        bool assert = true)
        where TEnum : struct, Enum
    {
        if (!typeof(TEnum).IsEnum)
            throw new ArgumentException($"{nameof(TEnum)} must be a 'enum' type");

        if (!EnumValuesHaveAttribute<TEnum>())
            throw new TechnicalCrashFailTestException($"All enum members of {nameof(TEnum)} must have a value");

        var view = getView(enumValue);
        if (view.Exists())
            return view;

        using var logContext = Log.GetDangerousDisposableNoLogsContext("Getting page state to calculate scroll position");

        var currentBlocksOrder = GetViewsOrder(getView);
        var currentIndex = GetScrollableIndex(enumValue);

        if (currentBlocksOrder[0] > currentIndex)
            parent.ScrollTo(view, scrollingStrategy: ScrollingStrategy.UpThenDown, assert: assert);
        else if (currentBlocksOrder.Last() < currentIndex)
            parent.ScrollTo(view, scrollingStrategy: ScrollingStrategy.Down, assert: assert);

        return getView(enumValue);
    }

    private static int GetScrollableIndex<TEnum>(TEnum enumValue)
        where TEnum : struct, Enum
    {
        return typeof(TEnum)
            .GetMember(enumValue.ToString())
            [0]
            .GetCustomAttribute<ScrollOrderAttribute>()
            .NotNull()
            .Index;
    }

    private static bool EnumValuesHaveAttribute<TEnum>()
        where TEnum : struct, Enum
    {
        return typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .All(field => field.GetCustomAttribute<ScrollOrderAttribute>() != null);
    }
}