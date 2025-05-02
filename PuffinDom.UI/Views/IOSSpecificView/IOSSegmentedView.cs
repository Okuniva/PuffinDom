using System.Runtime.CompilerServices;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Enums;
using PuffinDom.UI.Extensions;

namespace PuffinDom.UI.Views.IOSSpecificView;

// ReSharper disable once InconsistentNaming
public class IOSSegmentedView<TValueType> : View<IOSSegmentedView<TValueType>>
    where TValueType : struct, Enum
{
    private readonly Func<string, TValueType> _valueConverter;

    public IOSSegmentedView(
        View? parent,
        Func<string, TValueType> valueConverter,
        Func<string, string> query,
        bool wait = true,
        XPathStrategy xPathStrategy = XPathStrategy.Regular,
        [CallerMemberName] string viewName = "")
        : base(parent, query, wait, xPathStrategy, viewName)
    {
        _valueConverter = valueConverter;
    }

    private ListView<View> Buttons => new(
        this,
        x => x,
        x => x.ClosestChildren());

    public TValueType SelectedValue
    {
        get
        {
            var selectedValueText = Buttons.First(x => x.Selected).Text;

            Log.Write($"{this} — selected value: {selectedValueText}");

            return _valueConverter.Invoke(selectedValueText);
        }
    }
}