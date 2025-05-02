using System.Runtime.CompilerServices;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Asserts;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;
using Query = System.Func<string, string>;

namespace PuffinDom.UI.Views;

public class CheckBox : View<CheckBox>
{
    private readonly bool _turnOffForIOS;

    public CheckBox(
        View? parent,
        Query queryAndroid,
        Query queryIos,
        bool wait = true,
        bool turnOffForIOS = false,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(parent, queryAndroid, queryIos, wait, xPathStrategy, viewName)
    {
        _turnOffForIOS = turnOffForIOS;
    }

    public CheckBox(
        View? parent,
        Query query,
        bool wait = true,
        bool turnOffForIOS = false,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(
            parent,
            query,
            wait,
            xPathStrategy,
            viewName)
    {
        _turnOffForIOS = turnOffForIOS;
    }

    public bool IsChecked
    {
        get
        {
            if (UIContext.iOS && _turnOffForIOS)
                // TODO throw exception
                return false;

            return WaitViewData().NotNull().Checked;
        }
    }

    public CheckBox TapToSwitch(bool from, bool to, bool assertStatusAfterTap = true)
    {
        using var logContext = Log.PushContext($"Tapping {this} from {from} to {to}");

        if (UIContext.iOS && _turnOffForIOS)
            return base.Tap();

        this.AssertCheckedStatus(from);

        if (UIContext.Android)
            base.Tap();
        else
            this.Drag(
                from
                    ? Direction.Left
                    : Direction.Right);

        if (assertStatusAfterTap)
            this.AssertCheckedStatus(to);

        return this;
    }
}