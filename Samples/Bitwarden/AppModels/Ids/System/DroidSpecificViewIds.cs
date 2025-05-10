using System.Diagnostics.CodeAnalysis;

namespace Bitwarden.AppModels.Ids.System;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DroidSpecificViewIds
{
    public const string ChromeUrlBar = "com.android.chrome:id/url_bar";
    public const string AndroidChromiumBrowserUrlId = "org.chromium.webview_shell:id/url_field";
    public const string SettingsSearchButton = "com.android.settings:id/search";
    public const string SettingsSearchTextInput = "android:id/search_src_text";
    public const string SettingsHardKeyboardSwitch = "android:id/hard_keyboard_switch";
    public const string AndroidBrowserUrlId = "com.android.browser:id/url";
    public const string AndroidChromeUrlId = "com.android.chrome:id/url_bar";
    public const string AndroidPermissionAllowButton = "com.android.permissioncontroller:id/permission_allow_button";
    public const string AndroidSystemShareDialogContentId = "android:id/content_preview_text";
}