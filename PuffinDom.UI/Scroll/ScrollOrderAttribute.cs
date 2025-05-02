using Core.Tools.Disposables;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;

namespace PuffinDom.UI.Scroll;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
public class ScrollOrderAttribute : Attribute
{
    public ScrollOrderAttribute(
        int index,
        ScrollOrderElementType scrollOrderElementType = ScrollOrderElementType.Scrollable)
    {
        Index = index;
        ScrollOrderElementType = scrollOrderElementType;
    }

    public ScrollOrderAttribute(
        int indexAndroid,
        int indexIOS,
        ScrollOrderElementType scrollOrderElementType = ScrollOrderElementType.Scrollable)
        : this(UIContext.Android ? indexAndroid : indexIOS, scrollOrderElementType)
    {
    }

    public int Index { get; }

    public ScrollOrderElementType ScrollOrderElementType { get; private set; }

    public static bool OrderReceivingInProgress { get; private set; }

    public static IDisposable TurnOnDangerousCurrentViewOrderRegime()
    {
        Log.Write("Turning On dangerous view order receiving");

        if (OrderReceivingInProgress)
            throw new InvalidOperationException("Cannot turn off dangerous view order receiving");

        var startTime = DateTime.Now;
        OrderReceivingInProgress = true;

        return new DisposableObject().WhenDisposed(
            () =>
            {
                var passedTime = DateTime.Now - startTime;
                Log.Write($"Turning Off dangerous view order receiving in {passedTime.ToDisplayString()}");
                OrderReceivingInProgress = false;
            });
    }
}