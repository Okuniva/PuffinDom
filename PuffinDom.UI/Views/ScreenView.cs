using System.Runtime.CompilerServices;

namespace PuffinDom.UI.Views;

public abstract class ScreenView<TView> : View<TView>, IRootView
    where TView : ScreenView<TView>
{
    protected ScreenView(
        Func<string, string> query,
        bool wait = true)
        : base(
            null,
            query,
            wait,
            viewName: typeof(TView).Name)
    {
    }

    protected ScreenView(
        Func<string, string> droidQuery,
        Func<string, string> iOSQuery,
        bool wait = true)
        : base(
            null,
            droidQuery,
            iOSQuery,
            wait,
            viewName: typeof(TView).Name)
    {
    }

    protected ScreenView(
        View? parent,
        Func<string, string> droidQuery,
        Func<string, string> iOSQuery,
        bool wait = true,
        [CallerMemberName] string viewName = "")
        : base(
            parent,
            droidQuery,
            iOSQuery,
            wait,
            viewName: viewName)
    {
    }
}