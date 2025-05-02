using PuffinDom.Infrastructure.Appium;
using PuffinDom.UI.Extensions;

namespace PuffinDom.UI.Helpers;

public static class ViewDataExtensions
{
    private static string GetFullDescriptionForLog(this ViewData viewData)
    {
        string resultLog;

        if (viewData.Class == XPathStringExtensions.RootElementName)
            resultLog = "root view";
        else
        {
            resultLog = $"view '{viewData.Class}'";

            if (string.IsNullOrEmpty(viewData.Text))
                resultLog += " with no text";
            else
                resultLog += $" with text: '{viewData.Text}'";

            resultLog += viewData.Enabled
                ? ""
                : " it is disabled and";

            resultLog += viewData.Selected
                ? " it is selected and"
                : "";
        }

        resultLog += $" at x:{viewData.Rect.X},y:{viewData.Rect.Y},w:{viewData.Rect.Width},h:{viewData.Rect.Height}";
        if (!string.IsNullOrWhiteSpace(viewData.IdForLogs) && !viewData.IdForLogs.StartsWith("000000"))
            resultLog += $" and id:'{viewData.IdForLogs}'";

        return resultLog;
    }

    public static string GetFullDescriptionForLog(this List<ViewData> viewDataCollection)
    {
        if (viewDataCollection.Count == 0)
            return "No views found";

        var resultView = viewDataCollection[0];

        return viewDataCollection.Count == 1
            ? $"Found 1 {resultView.GetFullDescriptionForLog()}"
            : $"Found {viewDataCollection.Count} views. 1st is {resultView.GetFullDescriptionForLog()}; " +
              $"Other texts: {string.Join(", ", viewDataCollection.Skip(1).Select(x => x.Text))}";
    }
}