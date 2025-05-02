using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using PuffinDom.UI.Scroll;
using PuffinDom.UI.Views.IOSSpecificView;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Views;

public class View : IDisposable
{
    private readonly string _viewName;
    private readonly Query _viewQuery;
    private readonly XPathStrategy _xPathStrategy;

    public readonly bool GetFullPageSource;

    public View(
        View? parent,
        Query query,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
    {
        Parent = parent;
        _xPathStrategy = parent == null
            ? XPathStrategy.SkipParents
            : xPathStrategy;

        _viewName = viewName;
        ViewFullName = GenerateFullName(this);
        GetFullPageSource = GetType().GetCustomAttribute<GetFullXmlOnIOSAlwaysAttribute>() != null
                            || (Parent?.GetFullPageSource ?? false);

        if (GetFullPageSource)
            Log.Write($"View '{_viewName}' should be get with full page source");

        _viewQuery = query;

        if (!wait)
            return;

        if (ScrollOrderViewHelper.ScrollToViewWithScrollableIndex(this, _viewName))
            return;

        this.Wait();

        if (this is IRootView)
            ValidateFully();
    }

    public View(
        View? parent,
        Query droidQuery,
        Query iOSQuery,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : this(
            parent,
            UIContext.Android
                ? droidQuery
                : iOSQuery,
            wait,
            xPathStrategy,
            viewName)
    {
    }

    public View? Parent { get; }

    public string ViewFullName { get; }

    // ReSharper disable once MemberCanBeMadeStatic.Global
    protected DeviceManager Device => UIContext.Device;
    public Rect Rect => WaitViewData().NotNull().Rect;

    public bool Enabled
    {
        get
        {
            var enabled = WaitViewData().NotNull().Enabled;

            Log.Write(
                enabled
                    ? $"{this} is enabled"
                    : $"{this} is disabled");

            return enabled;
        }
    }

    public bool Selected => WaitViewData().NotNull().Selected;

    public virtual string Text
    {
        get
        {
            var text = WaitViewData().NotNull().Text;
            Log.Write($"'{text}' is text for {this}");
            return text;
        }
    }

    public Query FullQuery =>
        _xPathStrategy == XPathStrategy.SkipParents
            ? _viewQuery
            : Parent switch
            {
                IIosNotRootView when UIContext.iOS => _ => _viewQuery(Parent.FullQuery("")),
                IRootView => _ => _viewQuery(Parent.FullQuery("").RootParent()),
                _ => _ => _viewQuery(Parent.NotNull().FullQuery("")),
            };

    private static string GenerateFullName(View view)
    {
        var path = view._viewName;

        while (view.Parent != null)
        {
            view = view.Parent;

            path = path.StartsWith('[')
                ? $"{view._viewName}{path}"
                : $"{view._viewName}.{path}";
        }

        return path;
    }

    protected void ValidateFully()
    {
        Validate();
    }

    protected virtual void Validate()
    {
    }

    public virtual bool WaitSucceeded(bool assert = false, TimeSpan? timeOut = null)
    {
        return WaitViewData(assert, timeOut) != null;
    }

    protected ViewData? WaitViewData(bool assert = true, TimeSpan? timeOut = null)
    {
        var viewData = ViewWaitingStrategy.WaitViewData(
            this,
            timeOut);

        if (viewData == null)
            return assert
                ? throw new ViewNotFoundException($"{this} isn't on screen but should be")
                : null;

        return viewData;
    }

    public virtual View Tap(
        Point? customCoordinates = null,
        bool screenClosed = false,
        int times = 1)
    {
        try
        {
            var coordinates = customCoordinates ?? new Point(Rect.CenterX, Rect.CenterY);

            var where = customCoordinates == null
                ? " center"
                : $" [{coordinates.X}, {coordinates.Y}]";

            var howMany = times > 1
                ? $" {times} times"
                : "";

            using var logContext = Log.PushContext(
                $"Tapping {this}{where}{howMany}");

            Device.TapCoordinates(coordinates.X, coordinates.Y, times);
        }
        catch (Exception e)
        {
            Log.Write(e, $"Tapping {this} failed");

            throw new TechnicalCrashFailTestException($"Tapping {this} failed");
        }

        if (screenClosed)
            ValidateRootParentDisappeared();

        return this;
    }

    public void ValidateRootParentDisappeared()
    {
        GetRootParent()
            .Disappeared();
    }

    private View GetRootParent()
    {
        var viewToGetParent = this;
        while (true)
        {
            if (viewToGetParent.Parent == null)
                return viewToGetParent;

            viewToGetParent = viewToGetParent.Parent;
        }
    }

    public override string ToString() => ViewFullName;

    public virtual View Disappeared(string? customMessage = null, TimeSpan? timeout = null)
    {
        ViewWaitingStrategy.WaitDisappeared(
            this,
            timeout,
            customMessage: customMessage);

        return this;
    }

    public void Dispose()
    {
        Log.Write($"\nEnd of block {this}\n");
    }
}