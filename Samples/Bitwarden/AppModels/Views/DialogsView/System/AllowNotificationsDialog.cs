using Bitwarden.AppModels.Ids.System;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views.DialogsView.System;

public class AllowNotificationsDialog : ScreenView<AllowNotificationsDialog>
{
    public AllowNotificationsDialog(
        bool wait = true)
        : base(
            x => x.IdForSystemApp(DroidSpecificViewIds.AndroidPermissionAllowButton),
            x => x.Text("Allow"),
            wait)
    {
    }

    public View AllowButton => new(
        this,
        x => x.IdForSystemApp(DroidSpecificViewIds.AndroidPermissionAllowButton),
        x => x.Text("Allow"),
        xPathStrategy: XPathStrategy.SkipParents);
}