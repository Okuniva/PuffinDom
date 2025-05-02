using System.Runtime.CompilerServices;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Helpers;

namespace PuffinDom.UI.Views.IOSSpecificView;

// ReSharper disable once InconsistentNaming
public class iOSPickerWheel : View<iOSPickerWheel>
{
    public iOSPickerWheel(
        View? parent,
        bool wait = true,
        [CallerMemberName] string viewName = "")
        : base(parent, x => x.Class("XCUIElementTypePickerWheel"), wait, viewName: viewName)
    {
    }

    public iOSPickerWheel ChooseValue(string value)
    {
        using var logContext = Log.PushContext($"Scrolling to item '{value}' of iOS Picker Wheel");

        var currentValue = Text;
        var direction = Direction.Up;

        if (currentValue != value)
            ViewWaitingStrategy.WaitCondition(
                () =>
                {
                    this.Drag(direction, Rect.Height / 9);

                    if (currentValue == Text)
                        if (direction == Direction.Down)
                            throw new FailTestException($"'{value}' is not in the iOS Picker Wheel");
                        else
                            direction = Direction.Down;

                    currentValue = Text;

                    return currentValue == value;
                },
                $"Scrolling around to find '{value}' in {this}",
                true,
                10.Minutes());

        return this;
    }
}