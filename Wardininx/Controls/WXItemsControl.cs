#nullable enable
using Get.Data.Properties;
using Windows.UI.Xaml.Controls;
using Get.Data.Bindings;
using Windows.UI.Xaml;
using Get.Data.Collections;

namespace Wardininx.Controls;
abstract class WXItemsControlBase<T, TTemplate> : WXControl where TTemplate : class
{
    public OneWayUpdateCollectionProperty<T> ItemsSourceProperty { get; } = new();
    public IReadOnlyUpdateCollection<T> ItemsSource { get => ItemsSourceProperty.Value; set => ItemsSourceProperty.Value = value; }
    public Property<TTemplate?> ItemTemplateProperty { get; } = new(null);
    public TTemplate? ItemTemplate
    {
        get => ItemTemplateProperty.Value;
        set => ItemTemplateProperty.Value = value;
    }
    IDisposable? _collectionBinder;
    readonly IList<UIElement> _targetChildren;
    readonly UIElement _element;
    public WXItemsControlBase(UIElement element, IList<UIElement> children)
    {
        _element = element;
        _targetChildren = children;
        Template = App.GUIControlTemplate;
        ItemTemplateProperty.ValueChanged += (old, @new) =>
        {
            _collectionBinder?.Dispose();
            _targetChildren.Clear();
            if (ItemsSource is not null && @new is not null)
                _collectionBinder = Bind(ItemsSourceProperty, _targetChildren, @new);
        };
    }
    UserControl? TemplatedParent;
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        TemplatedParent = (UserControl)GetTemplateChild(App.GUIRootName);
        TemplatedParent.Content = _element;
    }
    protected abstract IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IList<UIElement> @out, TTemplate dataTemplate);
}

class WXItemsControl<T>(UIElement element, IList<UIElement> children) : WXItemsControlBase<T, DataTemplate<T, UIElement>>(element, children)
{
    public static PropertyDefinition<WXItemsControl<T>, IReadOnlyUpdateCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
    protected override IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IList<UIElement> @out, DataTemplate<T, UIElement> dataTemplate)
        => collection.Bind(@out, dataTemplate);
}

//struct WXSelectableItem<T>(IndexItem<T> item, WXSelectableItemsControl<T> source)
//{
//    public int Index => item.Index;
//    public T Value => item.Value;
//    public void Select()
//    {

//    }
//}
//class WXSelectableItemsControl<T>(UIElement element, IList<UIElement> children) : WXItemsControlBase<T, DataTemplate<IndexItem<T>, UIElement>>(element, children)
//{

//    public static PropertyDefinition<WXSelectableItemsControl<T>, IReadOnlyUpdateCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
//    protected override IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IList<UIElement> @out, DataTemplate<IndexItem<T>, UIElement> dataTemplate)
//        => collection.WithIndex().Bind(@out, dataTemplate);
//}

