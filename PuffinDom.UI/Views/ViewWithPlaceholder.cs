using System.Runtime.CompilerServices;
using PuffinDom.UI.Enums;

namespace PuffinDom.UI.Views;

public class ViewWithPlaceholder : ViewWithPlaceholder<ViewWithPlaceholder>
{
    public ViewWithPlaceholder(
        View? parent,
        Func<string, string> droidQuery,
        Func<string, string> iOSQuery,
        string placeholderString,
        PlaceholderPlatformFlags? placeholderPlatformFlags = PlaceholderPlatformFlags.Android | PlaceholderPlatformFlags.iOS,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : this(
            parent,
            UIContext.Android ? droidQuery : iOSQuery,
            placeholderString,
            placeholderPlatformFlags,
            wait,
            xPathStrategy,
            viewName)
    {
    }

    public ViewWithPlaceholder(
        View? parent,
        Func<string, string> query,
        string placeholderString,
        PlaceholderPlatformFlags? placeholderPlatformFlags = PlaceholderPlatformFlags.Android | PlaceholderPlatformFlags.iOS,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(
            parent,
            query,
            placeholderString,
            placeholderPlatformFlags,
            wait,
            xPathStrategy,
            viewName)
    {
    }
}