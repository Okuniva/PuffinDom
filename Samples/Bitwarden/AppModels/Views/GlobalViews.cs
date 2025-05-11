using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace Bitwarden.AppModels.Views;

public static class GlobalViews
{
    public static View CurrentActiveAccountIcon => new(
        null,
        x => x.Id("CurrentActiveAccount").TextLabelClass());

    public static View HeaderBarOptionsButton => new(
        null,
        x => x.Id("HeaderBarOptionsButton"));
}