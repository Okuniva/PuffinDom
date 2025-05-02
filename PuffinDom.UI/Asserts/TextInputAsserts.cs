using PuffinDom.UI.Enums;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Asserts;

public static class TextInputAsserts
{
    public static TextInput AssertCanContainAnyText(
        this TextInput textInput)
    {
        return textInput
            .EnterText("Test test", KeyboardDismissType.DoNot)
            .EnterText("123456 54", KeyboardDismissType.DoNot)
            .EnterText("@!$#%^ 44");
    }

    public static TextInput AssertCanNotContainText(
        this TextInput textInput)
    {
        if (!UIContext.AndroidTablet21)
            textInput.EnterText(" ", KeyboardDismissType.DoNot, expectPlaceholder: true);

        textInput
            .EnterText("Test", KeyboardDismissType.DoNot, expectPlaceholder: true)
            .EnterText("!@#%", expectedText: "1235");

        return textInput;
    }

    public static TextInput AssertCanContainDigits(
        this TextInput textInput)
    {
        return textInput
            .EnterText("1", KeyboardDismissType.DoNot)
            .EnterText("5");
    }

    public static TextInput AssertPlaceholderVisible(
        this TextInput textInput,
        string? message = null)
    {
        if (textInput.IsPlaceholderShouldBeVisible)
            return textInput.AssertTextWithPlaceholdersEquals(
                textInput.Placeholder,
                message);

        return textInput;
    }
}