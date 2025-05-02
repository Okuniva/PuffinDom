using System.Runtime.CompilerServices;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Exceptions;

namespace PuffinDom.UI.Views;

public abstract class ViewWithPlaceholder<TView> : View<TView>, IViewWithPlaceholder
    where TView : View<TView>
{
    private readonly PlaceholderPlatformFlags? _placeholderPlatformFlags;
    public readonly string Placeholder;

    protected ViewWithPlaceholder(
        View? parent,
        Func<string, string> query,
        string placeholderString,
        PlaceholderPlatformFlags? placeholderPlatformFlags = null,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(parent, query, wait, xPathStrategy, viewName)
    {
        Placeholder = placeholderString;
        _placeholderPlatformFlags = placeholderPlatformFlags;

        ValidatePlaceholderPlatformFlags();
    }

    public bool IsPlaceholderShouldBeVisible
    {
        get
        {
            if (!_placeholderPlatformFlags.HasValue)
            {
                Log.Write($"{this} placeholder should be visible");
                return true;
            }

            if (_placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.DoNotValidate))
            {
                Log.Write($"{this} placeholder shouldn't be validated");
                return false;
            }

            switch (UIContext.Platform)
            {
                case Platform.Android:
                    if (UIContext.AndroidTablet21)
                    {
                        var doNotValidateOnOldAndroid = _placeholderPlatformFlags.Value.HasFlag(
                            PlaceholderPlatformFlags.OnlyNewAndroid);

                        if (doNotValidateOnOldAndroid)
                            Log.Write($"{this} placeholder shouldn't be validated on old android");

                        return !doNotValidateOnOldAndroid;
                    }

                    Log.Write($"{this} placeholder should be visible");
                    return true;
                default:
                case Platform.iOS:
                    if (_placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.OnlyAndroid) ||
                        _placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.Android) ||
                        _placeholderPlatformFlags.Value.HasFlag(PlaceholderPlatformFlags.OnlyNewAndroid))
                    {
                        Log.Write($"{this} placeholder is only for Android. Skipping");
                        return false;
                    }

                    Log.Write($"{this} placeholder should be visible");
                    return true;
            }
        }
    }

    public string VisiblePlaceholder =>
        Placeholder.IsNotEmpty()
            ? $", {Placeholder}"
            : "";

    private void ValidatePlaceholderPlatformFlags()
    {
        if (_placeholderPlatformFlags is PlaceholderPlatformFlags.OnlyNewAndroid)
            throw new FailTestException($"Do not forget to add {PlaceholderPlatformFlags.iOS}");
    }
}