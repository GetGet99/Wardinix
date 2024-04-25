using Get.Data.Collections;
using Get.Data.Collections.Implementation;
using Get.Data.Collections.Update;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Wardininx;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Get.Data.DataTemplates;
using Get.Data.Collections.Conversion;

namespace Get.Data.Properties
{
    class DependencyPropertyDefinition<TOwnerType, TTargetType>(DependencyProperty dp) : PropertyDefinitionBase<TOwnerType, TTargetType> where TOwnerType : DependencyObject
    {
        public override PropertyBase<TTargetType> GetProperty(TOwnerType owner)
            => new DPPropertyWrapper<TOwnerType, TTargetType>(owner, dp);
    }
    class DPPropertyWrapper<TOwnerType, TTargetType> : PropertyBase<TTargetType>, IDisposable where TOwnerType : DependencyObject
    {
        readonly TOwnerType owner;
        readonly DependencyProperty dp;
        readonly long token;
        public DPPropertyWrapper(TOwnerType owner, DependencyProperty dp)
        {
            this.owner = owner;
            this.dp = dp;
            _Value = (TTargetType)owner.GetValue(dp);
            token = owner.RegisterPropertyChangedCallback(dp, delegate
            {
                var oldValue = _Value;
                var newValue = (TTargetType)owner.GetValue(dp);
                ValueChanging?.Invoke(oldValue, newValue);
                _Value = newValue;
                ValueChanged?.Invoke(oldValue, newValue);
            });
        }
        TTargetType _Value;
        public override TTargetType Value { get => _Value; set => owner.SetValue(dp, value); }

        public override event ValueChangingHandler<TTargetType> ValueChanging;
        public override event ValueChangedHandler<TTargetType> ValueChanged;
        public void Dispose()
        {
            owner.UnregisterPropertyChangedCallback(dp, token);
        }
        ~DPPropertyWrapper() => Dispose();
    }
    static class UWPExtension
    {
        public static DependencyPropertyDefinition<TOwnerType, TTargetType> AsPropertyDefinition<TOwnerType, TTargetType>(this DependencyProperty dp) where TOwnerType : DependencyObject
            => new(dp);
        public static DPPropertyWrapper<TOwnerType, TTargetType> AsProperty<TOwnerType, TTargetType>(this DependencyProperty dp, TOwnerType owner) where TOwnerType : DependencyObject
            => new(owner, dp);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<TSrc, T>(this UIElementCollection collection, CollectionItemsBinding<TSrc, UIElement> toBind)
        {
            toBind.Source.Bind(collection.AsGDCollection(), toBind.DataTemplate);
        }
    }
}
namespace Get.Data.Collections.Conversion
{
    static class UWPExtension
    {
        public static IGDCollection<UIElement> AsGDCollection(this UIElementCollection c)
        => new UIElementCollectionGDCollection(c);
        readonly struct UIElementCollectionGDCollection(UIElementCollection c) : IGDCollection<UIElement>, Get.Data.Collections.Implementation.IMoveImplGDCollection<UIElement>
        {
            public UIElement this[int index] { get => c[index]; set => c[index] = value; }

            public int Count => c.Count;

            public void Insert(int index, UIElement item)
                => c.Insert(index, item);

            public void RemoveAt(int index)
                => c.RemoveAt(index);
            public void Move(int index1, int index2)
                => c.Move((uint)index1, (uint)index2);
        }
    }
}
namespace Get.Data.XACL
{

    static class UWPExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<TSrc>(this UIElementCollection collection, CollectionItemsBinding<TSrc, UIElement> toBind)
        {
            toBind.Source.Bind(collection.AsGDCollection(), toBind.DataTemplate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<TSrc>(this UIElementCollection collection, IUpdateReadOnlyCollection<TSrc> source, DataTemplate<TSrc, UIElement> dataTemplate)
        {
            source.Bind(collection.AsGDCollection(), dataTemplate);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this UIElementCollection collection, IUpdateReadOnlyCollection<UIElement> source)
        {
            source.Bind(collection.AsGDCollection());
        }
    }
}