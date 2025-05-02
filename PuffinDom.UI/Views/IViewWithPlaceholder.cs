namespace PuffinDom.UI.Views;

public interface IViewWithPlaceholder
{
    public bool IsPlaceholderShouldBeVisible { get; }
    public string VisiblePlaceholder { get; }
}