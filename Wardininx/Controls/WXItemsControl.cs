#nullable enable
using Get.Data.Properties;
using Windows.UI.Xaml.Controls;
using Get.Data.Bindings;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;

namespace Wardininx.Controls;
abstract class WXItemsControlBase<T, TTemplate> : WXControl where TTemplate : class
{
    public Property<ObservableCollection<T>> ItemsSourceProperty { get; }
    public ObservableCollection<T> ItemsSource { get => ItemsSourceProperty.Value; set => ItemsSourceProperty.Value = value; }
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
        ItemsSourceProperty = new([]);
        Template = App.GUIControlTemplate;
        ItemTemplateProperty.ValueChanged += (old, @new) =>
        {
            _collectionBinder?.Dispose();
            _targetChildren.Clear();
            if (ItemsSource is not null && @new is not null)
                _collectionBinder = Bind(ItemsSource, _targetChildren, @new);
        };
        ItemsSourceProperty.ValueChanging += (old, @new) =>
        {
            _collectionBinder?.Dispose();
            _targetChildren.Clear();
            if (@new is not null && ItemTemplate is not null)
                _collectionBinder = Bind(@new, _targetChildren, ItemTemplate);
        };
    }
    UserControl? TemplatedParent;
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        TemplatedParent = (UserControl)GetTemplateChild(App.GUIRootName);
        TemplatedParent.Content = _element;
    }
    protected abstract IDisposable Bind(ObservableCollection<T> collection, IList<UIElement> @out, TTemplate dataTemplate);
}

class WXItemsControl<T>(UIElement element, IList<UIElement> children) : WXItemsControlBase<T, DataTemplate<T, UIElement>>(element, children)
{
    public static PropertyDefinition<WXItemsControl<T>, ObservableCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
    protected override IDisposable Bind(ObservableCollection<T> collection, IList<UIElement> @out, DataTemplate<T, UIElement> dataTemplate)
        => CollectionBinder.Bind(collection, @out, dataTemplate);
}


class WXItemsControlWithIndex<T>(UIElement element, IList<UIElement> children) : WXItemsControlBase<T, DataTemplate<(int Index, T Value), UIElement>>(element, children)
{
    public static PropertyDefinition<WXItemsControlWithIndex<T>, ObservableCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
    protected override IDisposable Bind(ObservableCollection<T> collection, IList<UIElement> @out, DataTemplate<(int Index, T Value), UIElement> dataTemplate)
        => CollectionBinder.Bind(collection, @out, dataTemplate);
}

