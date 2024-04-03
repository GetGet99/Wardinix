#nullable enable
using Get.Data.Properties;
using Windows.UI.Xaml.Controls;
using Get.Data.Bindings;
using Windows.UI.Xaml;

namespace Wardininx.Controls;

class WXContentControl<T> : WXControl
{
    public static PropertyDefinition<WXContentControl<T>, T> ContentPropertyDefinition { get; } =
        new(x => x.ContentProperty);
    public Property<T> ContentProperty { get; }
    public T Content { get => ContentProperty.Value; set => ContentProperty.Value = value; }
    public Property<DataTemplate<T, UIElement>?> ContentTemplateProperty { get; } = new(null);
    public DataTemplate<T, UIElement>? ContentTemplate {
        get => ContentTemplateProperty.Value;
        set => ContentTemplateProperty.Value = value;
    }
    DataTemplateGeneratedValue<T, UIElement>? _generatedValue;
    public WXContentControl(T defaultValue)
    {
        ContentProperty = new(defaultValue);
        Template = App.GUIControlTemplate;
        ContentTemplateProperty.ValueChanged += (old, @new) =>
        {
            if (old is not null && _generatedValue is not null)
                old.NotifyRecycle(_generatedValue);
            if (@new is not null && TemplatedParent is not null)
                TemplatedParent.Content =
                    (_generatedValue = @new.Generate(ContentProperty.ToBinding()))
                    .GeneratedValue;
        };
    }
    UserControl? TemplatedParent;
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        TemplatedParent = (UserControl)GetTemplateChild(App.GUIRootName);
        if (ContentTemplate is not null)
            TemplatedParent.Content =
                (_generatedValue = ContentTemplate.Generate(ContentProperty.ToBinding()))
                .GeneratedValue;
    }
}