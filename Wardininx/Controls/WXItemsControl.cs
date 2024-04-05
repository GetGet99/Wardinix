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

class WXSelectableItem<T>(Binding<IndexItem<T>> itemBinding, WXSelectableItemsControl<T> source)
{
    public Binding<IndexItem<T>> IndexItemBinding { get; } = itemBinding;
    public Binding<bool> IsSelected { get; } =
        source.SelectedIndexProperty.ToBinding().Combine(itemBinding, (x, item) => item.Index == x,
            delegate (bool output, ref int srcSelectedIdx, ref IndexItem<T> curItemIdx)
            {
                if (output && srcSelectedIdx != curItemIdx.Index)
                {
                    srcSelectedIdx = curItemIdx.Index;
                } else if (!output && srcSelectedIdx == curItemIdx.Index)
                {
                    srcSelectedIdx = -1;
                }
            });
    public void Select()
    {
        IsSelected.CurrentValue = true;
    }
}
class WXSelectableItemsControl<T> : WXItemsControlBase<T, DataTemplate<WXSelectableItem<T>, UIElement>>
{
    public WXSelectableItemsControl(UIElement element, IList<UIElement> children)
        : base(element, children)
    {
        SelectedValueProperty = new(_SelectedValueProperty);
        SelectedIndexProperty.ValueChanged += SelectedIndexProperty_ValueChanged;
        ItemsSourceProperty.ItemsAdded += ItemsSourceProperty_ItemsAdded;
        ItemsSourceProperty.ItemsRemoved += ItemsSourceProperty_ItemsRemoved;
        ItemsSourceProperty.ItemsMoved += ItemsSourceProperty_ItemsMoved;
        ItemsSourceProperty.ItemsReplaced += ItemsSourceProperty_ItemsReplaced;
    }


    private void ItemsSourceProperty_ItemsAdded(int startingIndex, IReadOnlyList<T> item)
    {
        if (startingIndex <= SelectedIndex)
            SelectedIndex += item.Count;
    }

    private void ItemsSourceProperty_ItemsRemoved(int startingIndex, IReadOnlyList<T> item)
    {
        if (startingIndex <= SelectedIndex)
        {
            if (SelectedIndex >= startingIndex + item.Count)
                SelectedIndex -= item.Count;
            else
                // the item that we selected got removed
                SelectedIndex = -1;
        }
    }
    private void ItemsSourceProperty_ItemsReplaced(int index, T oldItem, T newItem)
    {
        if (SelectedIndex == index) SelectedIndex = -1;
    }

    private void ItemsSourceProperty_ItemsMoved(int oldIndex, int newIndex, T oldIndexItem, T newIndexItem)
    {
        if (SelectedIndex == oldIndex)
            SelectedIndex = newIndex;
        if (SelectedIndex == newIndex)
            SelectedIndex = oldIndex;
    }

    private void SelectedIndexProperty_ValueChanged(int oldValue, int newValue)
    {
        if (newValue >= 0)
            _SelectedValueProperty.Value = ItemsSourceProperty[newValue];
        else
            _SelectedValueProperty.Value = default;
    }

    public static PropertyDefinition<WXSelectableItemsControl<T>, int> SelectedIndexPropertyDefnition { get; } = new(x => x.SelectedIndexProperty);
    public static PropertyDefinition<WXSelectableItemsControl<T>, T?> SelectedValuePropertyDefnition { get; } = new(x => x.SelectedValueProperty);
    public Property<int> SelectedIndexProperty { get; } = new(-1);
    
    readonly Property<T?> _SelectedValueProperty = new(default);
    
    public int SelectedIndex { get => SelectedIndexProperty.Value; set => SelectedIndexProperty.Value = value; }
    public ReadOnlyProperty<T?> SelectedValueProperty { get; }
    public T? SelectedValue => _SelectedValueProperty.Value;

    readonly UpdateCollection<WXSelectableItem<T>> tempCollection = [];
    public static PropertyDefinition<WXSelectableItemsControl<T>, IReadOnlyUpdateCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
    protected override IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IList<UIElement> @out, DataTemplate<WXSelectableItem<T>, UIElement> dataTemplate)
    {
        tempCollection.Clear();
        var a = collection.WithIndex().Bind(tempCollection, new(root => new(root, this)));
        var b = tempCollection.Bind(@out, dataTemplate);
        return new Get.Data.Bindings.Disposable(() =>
        {
            a.Dispose();
            b.Dispose();
            tempCollection.Clear();
        });
    }
}

