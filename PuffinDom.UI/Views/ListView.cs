using System.Collections;
using System.Runtime.CompilerServices;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Scroll;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Views;

public class ListView<TRowView> : View<ListView<TRowView>>, IReadOnlyList<TRowView>
    where TRowView : View
{
    protected readonly Query ItemQuery;

    public ListView(
        View? parent,
        Query query,
        Query itemQuery,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(parent, query, wait, xPathStrategy, viewName)
    {
        ItemQuery = itemQuery;
    }

    public ListView(
        View? parent,
        Query droidQuery,
        Query droidItemQuery,
        Query iOSQuery,
        Query iOSItemQuery,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : this(
            parent,
            UIContext.Android ? droidQuery : iOSQuery,
            UIContext.Android ? droidItemQuery : iOSItemQuery,
            wait,
            xPathStrategy,
            viewName)
    {
    }

    [Obsolete("Don't use Text property for ListView. It makes no sense")]
    public override string Text => throw
        new TechnicalCrashFailTestException(
            "Don't use Text property for ListView. It makes no sense");

    private List<TRowView> InnerList => InitializeList();

    public virtual IEnumerator<TRowView> GetEnumerator()
    {
        return InnerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count
    {
        get
        {
            var count = ViewExtensions.EvaluateXPath(
                    ToString(),
                    ItemQuery(FullQuery("")),
                    full: GetFullPageSource)
                .Count;

            Log.Write(
                count == 1
                    ? $"{this} has 1 item"
                    : $"{this} has {count} items");

            return count;
        }
    }

    public virtual TRowView this[int index]
    {
        get
        {
            ViewWaitingStrategy.WaitCondition(
                () => InnerList.Count > index,
                $"{this} has at least " +
                $"{index + 1} {(index > 1 ? "items" : "item")} " +
                $"Now it's {InnerList.Count} items",
                true);

            return InnerList[index];
        }
    }

    public TRowView First(
        Func<TRowView, bool> predicate)
    {
        ViewWaitingStrategy.WaitCondition(
            () => InnerList.FirstOrDefault(predicate) != null,
            $"Waiting {this} list has {typeof(TRowView).Name} with predicate");

        var result = InnerList.FirstOrDefault(predicate);

        if (result == null)
            throw new ViewNotFoundException(
                $"Failed to find {typeof(TRowView).Name} in {this} list with predicate");

        return result;
    }

    private List<TRowView> InitializeList()
    {
        using var logContext = Log.PushContext($"Getting {typeof(TRowView).Name} enumerator for {this}");

        var list = new List<TRowView>();

        var itemsCount = Count;
        for (var i = 0; i < itemsCount; i++)
            list.Add(GetRow(i));

        Log.Write(
            itemsCount == 1
                ? $"{this} has 1 item"
                : $"{this} has {itemsCount} items");

        return list;
    }

    public TRowView ScrollTo(
        Func<TRowView, bool> predicate,
        string failedMessage,
        ScrollingType scrollingType = ScrollingType.Vertical,
        ScrollingStrategy? scrollingStrategy = null,
        int maxScrolls = 5,
        Query? queryToGetElementsToCheckIfSomethingChangedAfterScroll = null,
        int? startYPxPosition = null)
    {
        var scrollHistory = ViewScrollingStrategy.ScrollTo(
            this,
            () => this.Any(predicate),
            maxScrolls,
            scrollingType,
            scrollingStrategy,
            message: failedMessage,
            queryToGetElementsToCheckIfSomethingChangedAfterScroll: queryToGetElementsToCheckIfSomethingChangedAfterScroll,
            startYPxPosition: startYPxPosition);

        var result = this.FirstOrDefault(predicate);

        if (result == null)
            throw new ViewNotFoundException(
                $"{failedMessage} | " +
                $"Failed to find in {this} after {scrollHistory.BackMovements} ↑ and {scrollHistory.ForwardMovements} ↓ moves");

        return result;
    }

    public void AssertRowDoesNotExistWithScrolling(Func<TRowView, bool> predicate, string message)
    {
        var scrollHistory = ViewScrollingStrategy.ScrollTo(
            this,
            () => this.Any(predicate),
            assert: false,
            message: message);

        var result = this.FirstOrDefault(predicate);

        if (result != null)
            throw new FailTestException(
                $"{message} | Row found in {this} but shouldn't be there after {scrollHistory.BackMovements} ↑ and {scrollHistory.ForwardMovements} ↓ moves");
    }

    protected virtual TRowView GetRow(int index)
    {
        TRowView? rowView = null;

        var itemQuery = new Query(x => ItemQuery(x).Index(index, true));

        var constructors = typeof(TRowView).GetConstructors();
        var twoQueriesConstructor = constructors
            .Any(
                x =>
                    x.GetParameters().Length == 5
                    && x.GetParameters().Count(p => p.ParameterType == typeof(Query)) == 2);

        var oneQueryConstructor = constructors.Any(x => x.GetParameters().Length == 4);
        var isRegularViewConstructor = constructors.Any(x => x.GetParameters().Length == 5);
        var rowName = $"[{index}]";
        const bool wait = false;

        if (twoQueriesConstructor)
            rowView = Activator.CreateInstance(
                typeof(TRowView),
                this,
                itemQuery,
                itemQuery,
                wait,
                rowName) as TRowView;
        else if (oneQueryConstructor)
            rowView = Activator.CreateInstance(
                typeof(TRowView),
                this,
                itemQuery,
                wait,
                rowName) as TRowView;
        else if (isRegularViewConstructor)
            rowView = Activator.CreateInstance(
                typeof(TRowView),
                this,
                itemQuery,
                wait,
                XPathStrategy.Regular,
                rowName) as TRowView;

        if (rowView == null)
            throw new FailTestException(
                $"Failed to create instance of {typeof(TRowView).Name}; " +
                $"Check that it has a constructor with the following signature: {typeof(TRowView).Name}(View parent, Func<object, AppQuery> query, bool isRoot, bool isGlobal, string viewName)");

        return rowView;
    }

    public View ScrollToRowWithText(
        string text,
        int maxScrolls = 11,
        ScrollingStrategy? scrollingStrategy = null,
        bool assert = true)
    {
        using var logContext = Log.PushContext($"Scrolling {this} → item with text '{text}'");

        return this.ScrollTo(
            new View(
                this,
                x => x.Text(text),
                false,
                XPathStrategy.Regular,
                $"Row['{text}']"),
            maxScrolls,
            scrollingStrategy: scrollingStrategy,
            assert: assert,
            queryToGetElementsToCheckIfSomethingChangedAfterScroll: x => ItemQuery(x).ElementAndAllChildren());
    }

    public ListView<TRowView> TapRowWithText(
        string text,
        int maxScrolls = 11,
        ScrollingStrategy scrollingStrategy = ScrollingStrategy.UpThenDown)
    {
        ScrollToRowWithText(text, maxScrolls, scrollingStrategy)
            .Tap();

        return this;
    }
}