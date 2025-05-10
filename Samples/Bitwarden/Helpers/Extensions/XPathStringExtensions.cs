using PuffinDom.Infrastructure.Helpers;
using PuffinDom.UI;
using PuffinDom.UI.Extensions;

namespace Bitwarden.Helpers.Extensions;

public static class XPathStringExtensions
{
    public static string ScreenWithTitle(this string query, string title)
    {
        switch (UIContext.Platform)
        {
            default:
            case Platform.Android:
                return query.IdAndText("PageTitleLabel", title);
            case Platform.iOS:
                throw new NotImplementedException();
        }
    }
}