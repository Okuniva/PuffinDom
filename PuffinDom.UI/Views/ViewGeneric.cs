using System.Drawing;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using PuffinDom.UI.Enums;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Views;

public abstract class View<TView> : View
    where TView : View<TView>
{
    protected View(
        View? parent,
        Query query,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(parent, query, wait, xPathStrategy, viewName)
    {
        ValidateGenericParameter();
    }

    protected View(
        View? parent,
        Query droidQuery,
        Query iOSQuery,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : this(parent, UIContext.Android ? droidQuery : iOSQuery, wait, xPathStrategy, viewName)
    {
    }

    private void ValidateGenericParameter()
    {
        Assert.That(this, Is.InstanceOf<TView>(), $"{this} should give itself as TView of its parent class");
    }

    public override TView Tap(
        Point? coordinates = null,
        bool screenClosed = false,
        int times = 1)
    {
        base.Tap(coordinates, screenClosed, times);

        return (TView)this;
    }
}