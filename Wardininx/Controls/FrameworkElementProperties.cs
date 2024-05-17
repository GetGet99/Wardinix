using Get.Data.Properties;
using System.Numerics;
using Windows.UI.Xaml;

namespace Wardininx.Controls;
public static class FrameworkElementProperties
{
    public static IReadOnlyPropertyDefinition<FrameworkElement, Vector2> ActualSizePropertyDefinition { get; } = new ActualSizePropertyDefinitionImpl();
    class ActualSizePropertyDefinitionImpl : IReadOnlyPropertyDefinition<FrameworkElement, Vector2>
    {
        public IReadOnlyProperty<Vector2> GetProperty(FrameworkElement owner)
        {
            var prop = new Property<Vector2>(owner.ActualSize);
            owner.SizeChanged += (_, _) => prop.Value = owner.ActualSize;
            return prop;
        }
    }

    public static T WithThemeResources<T>(this T ele, string localName, string globalName) where T : FrameworkElement
    {
        ele.Resources[localName] = Application.Current.Resources[globalName];
        ele.ActualThemeChanged += delegate
        {
            ele.Resources[localName] = Application.Current.Resources[globalName];
        };
        return ele;
    }
    public static T WithThemeResources<T>(this T ele, DependencyProperty property, string resourcesName) where T : FrameworkElement
    {
        if (!ele.Resources.TryGetValue(resourcesName, out var val)) val = Application.Current.Resources[resourcesName];
        ele.SetValue(property, val);
        ele.ActualThemeChanged += delegate
        {
            if (!ele.Resources.TryGetValue(resourcesName, out var val)) val = Application.Current.Resources[resourcesName];
            ele.SetValue(property, val);
        };
        return ele;
    }
    public static T WithThemeResources<T, TR>(this T any, Action<TR> setter, string resourcesName) where T : FrameworkElement
    {
        if (!any.Resources.TryGetValue(resourcesName, out var val)) val = Application.Current.Resources[resourcesName];
        setter((TR)val);
        any.ActualThemeChanged += delegate
        {
            if (!any.Resources.TryGetValue(resourcesName, out var val)) val = Application.Current.Resources[resourcesName];
            setter((TR)val);
        };
        return any;
    }
}