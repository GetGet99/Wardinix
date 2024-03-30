using System.Diagnostics;
using Wardininx;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

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
    static class Extension
    {
        public static DependencyPropertyDefinition<TOwnerType, TTargetType> AsPropertyDefinition<TOwnerType, TTargetType>(this DependencyProperty dp) where TOwnerType : DependencyObject
            => new(dp);
        public static DPPropertyWrapper<TOwnerType, TTargetType> AsProperty<TOwnerType, TTargetType>(this DependencyProperty dp, TOwnerType owner) where TOwnerType : DependencyObject
            => new(owner, dp);
    }
}
