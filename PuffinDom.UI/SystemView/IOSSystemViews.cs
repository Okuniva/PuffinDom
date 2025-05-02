using PuffinDom.UI.Extensions;
using PuffinDom.UI.SystemView.Ids;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.SystemView;

public class IOSSystemViews
{
    public static View KeyboardToolbarDoneButton => new(
        null,
        x => x.Id(SystemTouchIds.Toolbar).Id(SystemTouchIds.Done));

    public static View KeyboardDoneButton => new(
        null,
        x => x.Class(SystemTouchClasses.XCUIElementTypeKeyboard).Text(SystemTouchTexts.done));

    public static View KeyboardSearchButton => new(
        null,
        x => x.Class(SystemTouchClasses.XCUIElementTypeKeyboard)
            .Text(SystemTouchTexts.Search, SystemTouchTexts.search));

    public static View KeyboardNextButton => new(
        null,
        x => x.Class(SystemTouchClasses.XCUIElementTypeKeyboard).Text(SystemTouchTexts.next));

    public static View KeyboardReturnButton => new(
        null,
        x => x.Class(SystemTouchClasses.XCUIElementTypeKeyboard).Text(SystemTouchTexts.@return));

}