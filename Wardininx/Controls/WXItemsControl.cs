#nullable enable
using Get.Data.DataTemplates;
using Windows.UI.Xaml.Controls;
using Get.Data.Bindings;
using Windows.UI.Xaml;
using Get.Data.Collections;
using Get.Data.Collections.Update;
using Get.Data;
using Get.Data.Collections.Linq;
using Get.Data.Collections.Conversion;
using Get.Data.Bindings.Linq;

namespace Wardininx.Controls;
abstract class WXItemsControlBase<T, TTemplate> : WXControl where TTemplate : class
{
    public OneWayUpdateCollectionProperty<T> ItemsSourceProperty { get; } = new();
    public IUpdateReadOnlyCollection<T> ItemsSource { get => ItemsSourceProperty.Value; set => ItemsSourceProperty.Value = value; }
    public Property<TTemplate?> ItemTemplateProperty { get; } = new(null);
    public TTemplate? ItemTemplate
    {
        get => ItemTemplateProperty.Value;
        set => ItemTemplateProperty.Value = value;
    }
    IDisposable? _collectionBinder;
    readonly IGDCollection<UIElement> _targetChildren;
    readonly UIElement _element;
    public WXItemsControlBase(UIElement element, IGDCollection<UIElement> children)
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
    protected abstract IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IGDCollection<UIElement> @out, TTemplate dataTemplate);
}

class WXItemsControl<T>(UIElement element, IGDCollection<UIElement> children) : WXItemsControlBase<T, DataTemplate<T, UIElement>>(element, children)
{
    public WXItemsControl(UIElement element, IList<UIElement> children)
        : this(element, children.AsGDCollection()) { }
    public static PropertyDefinition<WXItemsControl<T>, IUpdateReadOnlyCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
    protected override IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IGDCollection<UIElement> @out, DataTemplate<T, UIElement> dataTemplate)
        => collection.Bind(@out, dataTemplate);
}

class PrimaryConstructorHelper
{
    public PrimaryConstructorHelper()
    {
        OnConstruct();
    }
    protected virtual void OnConstruct() { }
}
class WXSelectableItem<T>(IReadOnlyBinding<IndexItem<T>> itemBinding, WXSelectableItemsControl<T> source) : PrimaryConstructorHelper
{
    protected override void OnConstruct()
    {
        itemBinding.ValueChanged += delegate
        {

        };
    }
    public IReadOnlyBinding<IndexItem<T>> IndexItemBinding { get; } = itemBinding;
    public IBinding<bool> IsSelected { get; } =
        source.SelectedIndexProperty.Zip(itemBinding, (x, item) => item.Index == x,
            delegate (bool output, ref int srcSelectedIdx, IndexItem<T> curItemIdx)
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
        : this(element, children.AsGDCollection()) { }
    public WXSelectableItemsControl(UIElement element, IGDCollection<UIElement> children)
        : base(element, children)
    {
        SelectedValueProperty = new(_SelectedValueProperty);
        SelectedIndexProperty.ValueChanged += SelectedIndexProperty_ValueChanged;
        ItemsSourceProperty.ItemsChanged += ItemsSourceProperty_ItemsChanged;
        tempCollection.ItemsChanged += delegate
        {
            var a = this;
        };
    }

    private void ItemsSourceProperty_ItemsChanged(IEnumerable<IUpdateAction<T>> actions)
    {
        foreach (var action in actions)
        {

            switch (action)
            {
                case ItemsAddedUpdateAction<T> added:
                    if (added.StartingIndex <= SelectedIndex)
                        SelectedIndex += added.Items.Count;
                    break;
                case ItemsRemovedUpdateAction<T> removed:
                    if (removed.StartingIndex <= SelectedIndex)
                    {
                        if (SelectedIndex >= removed.StartingIndex + removed.Items.Count)
                            SelectedIndex -= removed.Items.Count;
                        else
                            // the item that we selected got removed
                            SelectedIndex = -1;
                    }
                    break;
                case ItemsMovedUpdateAction<T> moved:
                    if (SelectedIndex == moved.OldIndex) SelectedIndex = moved.NewIndex;
                    if (SelectedIndex == moved.NewIndex) SelectedIndex = moved.OldIndex;
                    break;
                case ItemsReplacedUpdateAction<T> replaced:
                    if (SelectedIndex == replaced.Index) SelectedIndex = -1;
                    break;
            }
        }
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

    readonly UpdateCollection<WXSelectableItem<T>> tempCollection = new();
    public static PropertyDefinition<WXSelectableItemsControl<T>, IUpdateReadOnlyCollection<T>> ItemsSourcePropertyDefinition { get; } = new(x => x.ItemsSourceProperty);
    protected override IDisposable Bind(OneWayUpdateCollectionProperty<T> collection, IGDCollection<UIElement> @out, DataTemplate<WXSelectableItem<T>, UIElement> dataTemplate)
    {
        tempCollection.Clear();
        var a = collection.AsUpdateReadOnly().WithIndex().Bind(tempCollection, new(root => new(root, this)));
        var b = tempCollection.Bind(@out, dataTemplate);
        return new Disposable(() =>
        {
            a.Dispose();
            b.Dispose();
            tempCollection.Clear();
        });
    }
}

