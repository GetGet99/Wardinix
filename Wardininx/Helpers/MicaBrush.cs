#pragma warning disable
// Mica Brush by Dongle
// https://discord.com/channels/714581497222398064/927947850120302703/1031029264264466463
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BlurredWallpaperMaterials;

public enum BackdropKind
{
    Base = 0,
    BaseAlt = 1,
    Custom = 2
}

partial class BackdropMaterial : XamlCompositionBrushBase
{
    // MODIFIED
    private Compositor _compositor;
    // END MODIFIED

    private double _tintOpacity;

    private double _luminosityOpacity;

    private Color _tintColor;

    private Color _luminosityColor;

    private ElementTheme _theme;

    public ElementTheme Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            UpdateTheme();
        }
    }

    private BackdropKind _kind;

    public int Kind
    {
        get => (int)_kind;
        set
        {
            SetProperty(ref _kind, (BackdropKind)value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsKindCustom)));
            UpdateTheme();
        }
    }

    public bool IsKindCustom => _kind == BackdropKind.Custom;

    partial void OnLuminosityColorChanged(Color _) => UpdateBrush();

    partial void OnTintColorChanged(Color _) => UpdateBrush();

    partial void OnTintOpacityChanged(double _) => UpdateBrush();

    partial void OnLuminosityOpacityChanged(double _) => UpdateBrush();

    private void UpdateTheme()
    {
        switch (_kind)
        {
            case BackdropKind.Custom:
                return;
            case BackdropKind.Base:
                switch (Theme)
                {
                    case ElementTheme.Light:
                        _tintColor = _luminosityColor = Color.FromArgb(255, 243, 243, 243);
                        _tintOpacity = 50;
                        _luminosityOpacity = 100;
                        break;
                    case ElementTheme.Dark:
                        _tintColor = _luminosityColor = Color.FromArgb(255, 32, 32, 32);
                        _tintOpacity = 80;
                        _luminosityOpacity = 100;
                        break;
                }
                break;
            case BackdropKind.BaseAlt:
                switch (Theme)
                {
                    case ElementTheme.Light:
                        _tintColor = _luminosityColor = Color.FromArgb(255, 218, 218, 218);
                        _tintOpacity = 50;
                        _luminosityOpacity = 100;
                        break;
                    case ElementTheme.Dark:
                        _tintColor = _luminosityColor = Color.FromArgb(255, 10, 10, 10);
                        _tintOpacity = 0;
                        _luminosityOpacity = 100;
                        break;
                }
                break;
        }

        UpdateBrush();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TintColor)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TintOpacity)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LuminosityColor)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LuminosityOpacity)));
    }

    public static CompositionBrush BuildMicaEffectBrush(Compositor compositor, Color tintColor, float tintOpacity, Color luminosityColor, float luminosityOpacity)
    {
        // Tint Color.

        var tintColorEffect = new ColorSourceEffect();
        tintColorEffect.Name = "TintColor";
        tintColorEffect.Color = tintColor;

        // OpacityEffect applied to Tint.
        var tintOpacityEffect = new OpacityEffect();
        tintOpacityEffect.Name = "TintOpacity";
        tintOpacityEffect.Opacity = tintOpacity;
        tintOpacityEffect.Source = tintColorEffect;

        // Apply Luminosity:

        // Luminosity Color.
        var luminosityColorEffect = new ColorSourceEffect();
        luminosityColorEffect.Color = luminosityColor;

        // OpacityEffect applied to Luminosity.
        var luminosityOpacityEffect = new OpacityEffect();
        luminosityOpacityEffect.Name = "LuminosityOpacity";
        luminosityOpacityEffect.Opacity = luminosityOpacity;
        luminosityOpacityEffect.Source = luminosityColorEffect;

        // Luminosity Blend.
        // NOTE: There is currently a bug where the names of BlendEffectMode::Luminosity and BlendEffectMode::Color are flipped.
        var luminosityBlendEffect = new BlendEffect();
        luminosityBlendEffect.Mode = BlendEffectMode.Color;
        luminosityBlendEffect.Background = new CompositionEffectSourceParameter("BlurredWallpaperBackdrop");
        luminosityBlendEffect.Foreground = luminosityOpacityEffect;

        // Apply Tint:

        // Color Blend.
        // NOTE: There is currently a bug where the names of BlendEffectMode::Luminosity and BlendEffectMode::Color are flipped.
        var colorBlendEffect = new BlendEffect();
        colorBlendEffect.Mode = BlendEffectMode.Luminosity;
        colorBlendEffect.Background = luminosityBlendEffect;
        colorBlendEffect.Foreground = tintOpacityEffect;

        CompositionEffectBrush micaEffectBrush = compositor.CreateEffectFactory(colorBlendEffect).CreateBrush();
        //var blurredWallpaperBackdropBrush = (ICompositorWithBlurredWallpaperBackdropBrush)((object)compositor); // Code for < 22000 SDK
        //micaEffectBrush.SetSourceParameter("BlurredWallpaperBackdrop", blurredWallpaperBackdropBrush.TryCreateBlurredWallpaperBackdropBrush());
        micaEffectBrush.SetSourceParameter("BlurredWallpaperBackdrop", compositor.TryCreateBlurredWallpaperBackdropBrush());

        return micaEffectBrush;
    }

    private CompositionBrush CreateCrossFadeEffectBrush(Compositor compositor, CompositionBrush from, CompositionBrush to)
    {
        var crossFadeEffect = new CrossFadeEffect();
        crossFadeEffect.Name = "Crossfade"; // Name to reference when starting the animation.
        crossFadeEffect.Source1 = new CompositionEffectSourceParameter("source1");
        crossFadeEffect.Source2 = new CompositionEffectSourceParameter("source2");
        crossFadeEffect.CrossFade = 0;

        CompositionEffectBrush crossFadeEffectBrush = compositor.CreateEffectFactory(crossFadeEffect, new List<string>() { "Crossfade.CrossFade" }).CreateBrush();
        crossFadeEffectBrush.Comment = "Crossfade";
        // The inputs have to be swapped here to work correctly...
        crossFadeEffectBrush.SetSourceParameter("source1", to);
        crossFadeEffectBrush.SetSourceParameter("source2", from);
        return crossFadeEffectBrush;
    }

    private ScalarKeyFrameAnimation CreateCrossFadeAnimation(Compositor compositor)
    {
        ScalarKeyFrameAnimation animation = compositor.CreateScalarKeyFrameAnimation();
        LinearEasingFunction linearEasing = compositor.CreateLinearEasingFunction();
        animation.InsertKeyFrame(0.0f, 0.0f, linearEasing);
        animation.InsertKeyFrame(1.0f, 1.0f, linearEasing);
        animation.Duration = TimeSpan.FromMilliseconds(250);
        return animation;
    }

    private void UpdateBrush()
    {
        // MODIFIED
        Compositor compositor = _compositor ?? Window.Current.Compositor;
        // END MODIFIED

        CompositionBrush newBrush = BuildMicaEffectBrush(compositor, TintColor, (float)(TintOpacity / 100), LuminosityColor, (float)(LuminosityOpacity / 100));

        CompositionBrush oldBrush = CompositionBrush;

        if (oldBrush == null || CompositionBrush.Comment == "Crossfade" || Kind == (int)BackdropKind.Custom)
        {
            // Set new brush directly
            if (oldBrush != null)
            {
                oldBrush.Dispose();
            }
            this.CompositionBrush = newBrush;
        }
        else
        {
            // Crossfade
            CompositionBrush crossFadeBrush = CreateCrossFadeEffectBrush(compositor, oldBrush, newBrush);
            ScalarKeyFrameAnimation animation = CreateCrossFadeAnimation(compositor);
            CompositionBrush = crossFadeBrush;

            var crossFadeAnimationBatch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            crossFadeBrush.StartAnimation("CrossFade.CrossFade", animation);
            crossFadeAnimationBatch.End();

            crossFadeAnimationBatch.Completed += (o, a) =>
            {
                crossFadeBrush.Dispose();
                oldBrush.Dispose();
                this.CompositionBrush = newBrush;
            };
        }
    }

    protected override void OnConnected()
    {
        if (DesignMode.DesignModeEnabled)
        {
            CompositionBrush = Window.Current.Compositor.CreateColorBrush(Color.FromArgb(255, 243, 243, 243));
            return;
        }

        UpdateBrush();
    }
}
partial class BackdropMaterial
{
    /// <inheritdoc cref="_compositor"/>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public global::Windows.UI.Composition.Compositor Compositor
    {
        get => _compositor;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<global::Windows.UI.Composition.Compositor>.Default.Equals(_compositor, value))
            {
                OnCompositorChanging(value);
                _compositor = value;
                OnCompositorChanged(value);
                OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Compositor);
            }
        }
    }

    /// <inheritdoc cref="_tintOpacity"/>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public double TintOpacity
    {
        get => _tintOpacity;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<double>.Default.Equals(_tintOpacity, value))
            {
                OnTintOpacityChanging(value);
                _tintOpacity = value;
                OnTintOpacityChanged(value);
                OnPropertyChanged(__KnownINotifyPropertyChangedArgs.TintOpacity);
            }
        }
    }

    /// <inheritdoc cref="_luminosityOpacity"/>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public double LuminosityOpacity
    {
        get => _luminosityOpacity;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<double>.Default.Equals(_luminosityOpacity, value))
            {
                OnLuminosityOpacityChanging(value);
                _luminosityOpacity = value;
                OnLuminosityOpacityChanged(value);
                OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LuminosityOpacity);
            }
        }
    }

    /// <inheritdoc cref="_tintColor"/>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public global::Windows.UI.Color TintColor
    {
        get => _tintColor;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<global::Windows.UI.Color>.Default.Equals(_tintColor, value))
            {
                OnTintColorChanging(value);
                _tintColor = value;
                OnTintColorChanged(value);
                OnPropertyChanged(__KnownINotifyPropertyChangedArgs.TintColor);
            }
        }
    }

    /// <inheritdoc cref="_luminosityColor"/>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public global::Windows.UI.Color LuminosityColor
    {
        get => _luminosityColor;
        set
        {
            if (!global::System.Collections.Generic.EqualityComparer<global::Windows.UI.Color>.Default.Equals(_luminosityColor, value))
            {
                OnLuminosityColorChanging(value);
                _luminosityColor = value;
                OnLuminosityColorChanged(value);
                OnPropertyChanged(__KnownINotifyPropertyChangedArgs.LuminosityColor);
            }
        }
    }

    /// <summary>Executes the logic for when <see cref="Compositor"/> is changing.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnCompositorChanging(global::Windows.UI.Composition.Compositor value);
    /// <summary>Executes the logic for when <see cref="Compositor"/> just changed.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnCompositorChanged(global::Windows.UI.Composition.Compositor value);
    /// <summary>Executes the logic for when <see cref="TintOpacity"/> is changing.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnTintOpacityChanging(double value);
    /// <summary>Executes the logic for when <see cref="TintOpacity"/> just changed.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnTintOpacityChanged(double value);
    /// <summary>Executes the logic for when <see cref="LuminosityOpacity"/> is changing.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnLuminosityOpacityChanging(double value);
    /// <summary>Executes the logic for when <see cref="LuminosityOpacity"/> just changed.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnLuminosityOpacityChanged(double value);
    /// <summary>Executes the logic for when <see cref="TintColor"/> is changing.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnTintColorChanging(global::Windows.UI.Color value);
    /// <summary>Executes the logic for when <see cref="TintColor"/> just changed.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnTintColorChanged(global::Windows.UI.Color value);
    /// <summary>Executes the logic for when <see cref="LuminosityColor"/> is changing.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnLuminosityColorChanging(global::Windows.UI.Color value);
    /// <summary>Executes the logic for when <see cref="LuminosityColor"/> just changed.</summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    partial void OnLuminosityColorChanged(global::Windows.UI.Color value);
}
// END ORIGINAL CODE
// AUTOGENERATED CODE
partial class BackdropMaterial : global::System.ComponentModel.INotifyPropertyChanged
{
    /// <inheritdoc cref = "global::System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Raises the <see cref = "PropertyChanged"/> event.
    /// </summary>
    /// <param name = "e">The input <see cref = "global::System.ComponentModel.PropertyChangedEventArgs"/> instance.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected virtual void OnPropertyChanged(global::System.ComponentModel.PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref = "PropertyChanged"/> event.
    /// </summary>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Compares the current and new values for a given property. If the value has changed, updates
    /// the property with the new value, then raises the <see cref = "PropertyChanged"/> event.
    /// </summary>
    /// <typeparam name = "T">The type of the property that changed.</typeparam>
    /// <param name = "field">The field storing the property's value.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are the same.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetProperty<T>(ref T field, T newValue, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given property. If the value has changed, updates
    /// the property with the new value, then raises the <see cref = "PropertyChanged"/> event.
    /// See additional notes about this overload in <see cref = "SetProperty{T}(ref T, T, string)"/>.
    /// </summary>
    /// <typeparam name = "T">The type of the property that changed.</typeparam>
    /// <param name = "field">The field storing the property's value.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "comparer">The <see cref = "global::System.Collections.Generic.IEqualityComparer{T}"/> instance to use to compare the input values.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetProperty<T>(ref T field, T newValue, global::System.Collections.Generic.IEqualityComparer<T> comparer, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (comparer.Equals(field, newValue))
        {
            return false;
        }

        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given property. If the value has changed, updates
    /// the property with the new value, then raises the <see cref = "PropertyChanged"/> event.
    /// This overload is much less efficient than <see cref = "SetProperty{T}(ref T, T, string)"/> and it
    /// should only be used when the former is not viable (eg. when the target property being
    /// updated does not directly expose a backing field that can be passed by reference).
    /// For performance reasons, it is recommended to use a stateful callback if possible through
    /// the <see cref = "SetProperty{TModel, T}(T, T, TModel, global::System.Action{TModel, T}, string? )"/> whenever possible
    /// instead of this overload, as that will allow the C# compiler to cache the input callback and
    /// reduce the memory allocations. More info on that overload are available in the related XML
    /// docs. This overload is here for completeness and in cases where that is not applicable.
    /// </summary>
    /// <typeparam name = "T">The type of the property that changed.</typeparam>
    /// <param name = "oldValue">The current property value.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "callback">A callback to invoke to update the property value.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are the same.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetProperty<T>(T oldValue, T newValue, global::System.Action<T> callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            return false;
        }

        callback(newValue);
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given property. If the value has changed, updates
    /// the property with the new value, then raises the <see cref = "PropertyChanged"/> event.
    /// See additional notes about this overload in <see cref = "SetProperty{T}(T, T, global::System.Action{T}, string)"/>.
    /// </summary>
    /// <typeparam name = "T">The type of the property that changed.</typeparam>
    /// <param name = "oldValue">The current property value.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "comparer">The <see cref = "global::System.Collections.Generic.IEqualityComparer{T}"/> instance to use to compare the input values.</param>
    /// <param name = "callback">A callback to invoke to update the property value.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetProperty<T>(T oldValue, T newValue, global::System.Collections.Generic.IEqualityComparer<T> comparer, global::System.Action<T> callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (comparer.Equals(oldValue, newValue))
        {
            return false;
        }

        callback(newValue);
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given nested property. If the value has changed,
    /// updates the property and then raises the <see cref = "PropertyChanged"/> event.
    /// The behavior mirrors that of <see cref = "SetProperty{T}(ref T, T, string)"/>,
    /// with the difference being that this method is used to relay properties from a wrapped model in the
    /// current instance. This type is useful when creating wrapping, bindable objects that operate over
    /// models that lack support for notification (eg. for CRUD operations).
    /// Suppose we have this model (eg. for a database row in a table):
    /// <code>
    /// public class Person
    /// {
    ///     public string Name { get; set; }
    /// }
    /// </code>
    /// We can then use a property to wrap instances of this type into our observable model (which supports
    /// notifications), injecting the notification to the properties of that model, like so:
    /// <code>
    /// [INotifyPropertyChanged]
    /// public partial class BindablePerson
    /// {
    ///     public Model { get; }
    ///
    ///     public BindablePerson(Person model)
    ///     {
    ///         Model = model;
    ///     }
    ///
    ///     public string Name
    ///     {
    ///         get => Model.Name;
    ///         set => Set(Model.Name, value, Model, (model, name) => model.Name = name);
    ///     }
    /// }
    /// </code>
    /// This way we can then use the wrapping object in our application, and all those "proxy" properties will
    /// also raise notifications when changed. Note that this method is not meant to be a replacement for
    /// <see cref = "SetProperty{T}(ref T, T, string)"/>, and it should only be used when relaying properties to a model that
    /// doesn't support notifications, and only if you can't implement notifications to that model directly (eg. by having
    /// it implement <see cref = "global::System.ComponentModel.INotifyPropertyChanged"/>). The syntax relies on passing the target model and a stateless callback
    /// to allow the C# compiler to cache the function, which results in much better performance and no memory usage.
    /// </summary>
    /// <typeparam name = "TModel">The type of model whose property (or field) to set.</typeparam>
    /// <typeparam name = "T">The type of property (or field) to set.</typeparam>
    /// <param name = "oldValue">The current property value.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "model">The model containing the property being updated.</param>
    /// <param name = "callback">The callback to invoke to set the target property value, if a change has occurred.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are the same.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetProperty<TModel, T>(T oldValue, T newValue, TModel model, global::System.Action<TModel, T> callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        where TModel : class
    {
        if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            return false;
        }

        callback(model, newValue);
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given nested property. If the value has changed,
    /// updates the property and then raises the <see cref = "PropertyChanged"/> event.
    /// The behavior mirrors that of <see cref = "SetProperty{T}(ref T, T, string)"/>,
    /// with the difference being that this method is used to relay properties from a wrapped model in the
    /// current instance. See additional notes about this overload in <see cref = "SetProperty{TModel, T}(T, T, TModel, global::System.Action{TModel, T}, string)"/>.
    /// </summary>
    /// <typeparam name = "TModel">The type of model whose property (or field) to set.</typeparam>
    /// <typeparam name = "T">The type of property (or field) to set.</typeparam>
    /// <param name = "oldValue">The current property value.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "comparer">The <see cref = "global::System.Collections.Generic.IEqualityComparer{T}"/> instance to use to compare the input values.</param>
    /// <param name = "model">The model containing the property being updated.</param>
    /// <param name = "callback">The callback to invoke to set the target property value, if a change has occurred.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetProperty<TModel, T>(T oldValue, T newValue, global::System.Collections.Generic.IEqualityComparer<T> comparer, TModel model, global::System.Action<TModel, T> callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        where TModel : class
    {
        if (comparer.Equals(oldValue, newValue))
        {
            return false;
        }

        callback(model, newValue);
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Compares the current and new values for a given field (which should be the backing field for a property).
    /// If the value has changed, updates the field and then raises the <see cref = "PropertyChanged"/> event.
    /// The behavior mirrors that of <see cref = "SetProperty{T}(ref T, T, string)"/>, with the difference being that
    /// this method will also monitor the new value of the property (a generic <see cref = "global::System.Threading.Tasks.Task"/>) and will also
    /// raise the <see cref = "PropertyChanged"/> again for the target property when it completes.
    /// This can be used to update bindings observing that <see cref = "global::System.Threading.Tasks.Task"/> or any of its properties.
    /// This method and its overload specifically rely on the <see cref = "TaskNotifier"/> type, which needs
    /// to be used in the backing field for the target <see cref = "global::System.Threading.Tasks.Task"/> property. The field doesn't need to be
    /// initialized, as this method will take care of doing that automatically. The <see cref = "TaskNotifier"/>
    /// type also includes an implicit operator, so it can be assigned to any <see cref = "global::System.Threading.Tasks.Task"/> instance directly.
    /// Here is a sample property declaration using this method:
    /// <code>
    /// private TaskNotifier myTask;
    ///
    /// public Task MyTask
    /// {
    ///     get => myTask;
    ///     private set => SetAndNotifyOnCompletion(ref myTask, value);
    /// }
    /// </code>
    /// </summary>
    /// <param name = "taskNotifier">The field notifier to modify.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are
    /// the same. The return value being <see langword="true"/> only indicates that the new value being assigned to
    /// <paramref name = "taskNotifier"/> is different than the previous one, and it does not mean the new
    /// <see cref = "global::System.Threading.Tasks.Task"/> instance passed as argument is in any particular state.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetPropertyAndNotifyOnCompletion(ref TaskNotifier? taskNotifier, global::System.Threading.Tasks.Task? newValue, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        return SetPropertyAndNotifyOnCompletion(taskNotifier ??= new TaskNotifier(), newValue, null, propertyName);
    }

    /// <summary>
    /// Compares the current and new values for a given field (which should be the backing field for a property).
    /// If the value has changed, updates the field and then raises the <see cref = "PropertyChanged"/> event.
    /// This method is just like <see cref = "SetPropertyAndNotifyOnCompletion(ref TaskNotifier, global::System.Threading.Tasks.Task, string)"/>,
    /// with the difference being an extra <see cref = "global::System.Action{T}"/> parameter with a callback being invoked
    /// either immediately, if the new task has already completed or is <see langword="null"/>, or upon completion.
    /// </summary>
    /// <param name = "taskNotifier">The field notifier to modify.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "callback">A callback to invoke to update the property value.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are the same.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetPropertyAndNotifyOnCompletion(ref TaskNotifier? taskNotifier, global::System.Threading.Tasks.Task? newValue, global::System.Action<global::System.Threading.Tasks.Task?> callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        return SetPropertyAndNotifyOnCompletion(taskNotifier ??= new TaskNotifier(), newValue, callback, propertyName);
    }

    /// <summary>
    /// Compares the current and new values for a given field (which should be the backing field for a property).
    /// If the value has changed, updates the field and then raises the <see cref = "PropertyChanged"/> event.
    /// The behavior mirrors that of <see cref = "SetProperty{T}(ref T, T, string)"/>, with the difference being that
    /// this method will also monitor the new value of the property (a generic <see cref = "global::System.Threading.Tasks.Task"/>) and will also
    /// raise the <see cref = "PropertyChanged"/> again for the target property when it completes.
    /// This can be used to update bindings observing that <see cref = "global::System.Threading.Tasks.Task"/> or any of its properties.
    /// This method and its overload specifically rely on the <see cref = "TaskNotifier{T}"/> type, which needs
    /// to be used in the backing field for the target <see cref = "global::System.Threading.Tasks.Task"/> property. The field doesn't need to be
    /// initialized, as this method will take care of doing that automatically. The <see cref = "TaskNotifier{T}"/>
    /// type also includes an implicit operator, so it can be assigned to any <see cref = "global::System.Threading.Tasks.Task"/> instance directly.
    /// Here is a sample property declaration using this method:
    /// <code>
    /// private TaskNotifier&lt;int&gt; myTask;
    ///
    /// public Task&lt;int&gt; MyTask
    /// {
    ///     get => myTask;
    ///     private set => SetAndNotifyOnCompletion(ref myTask, value);
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name = "T">The type of result for the <see cref = "global::System.Threading.Tasks.Task{TResult}"/> to set and monitor.</typeparam>
    /// <param name = "taskNotifier">The field notifier to modify.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are
    /// the same. The return value being <see langword="true"/> only indicates that the new value being assigned to
    /// <paramref name = "taskNotifier"/> is different than the previous one, and it does not mean the new
    /// <see cref = "global::System.Threading.Tasks.Task{TResult}"/> instance passed as argument is in any particular state.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetPropertyAndNotifyOnCompletion<T>(ref TaskNotifier<T>? taskNotifier, global::System.Threading.Tasks.Task<T>? newValue, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        return SetPropertyAndNotifyOnCompletion(taskNotifier ??= new TaskNotifier<T>(), newValue, null, propertyName);
    }

    /// <summary>
    /// Compares the current and new values for a given field (which should be the backing field for a property).
    /// If the value has changed, updates the field and then raises the <see cref = "PropertyChanged"/> event.
    /// This method is just like <see cref = "SetPropertyAndNotifyOnCompletion{T}(ref TaskNotifier{T}, global::System.Threading.Tasks.Task{T}, string)"/>,
    /// with the difference being an extra <see cref = "global::System.Action{T}"/> parameter with a callback being invoked
    /// either immediately, if the new task has already completed or is <see langword="null"/>, or upon completion.
    /// </summary>
    /// <typeparam name = "T">The type of result for the <see cref = "global::System.Threading.Tasks.Task{TResult}"/> to set and monitor.</typeparam>
    /// <param name = "taskNotifier">The field notifier to modify.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "callback">A callback to invoke to update the property value.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    /// <remarks>
    /// The <see cref = "PropertyChanged"/> event is not raised if the current and new value for the target property are the same.
    /// </remarks>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected bool SetPropertyAndNotifyOnCompletion<T>(ref TaskNotifier<T>? taskNotifier, global::System.Threading.Tasks.Task<T>? newValue, global::System.Action<global::System.Threading.Tasks.Task<T>?> callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        return SetPropertyAndNotifyOnCompletion(taskNotifier ??= new TaskNotifier<T>(), newValue, callback, propertyName);
    }

    /// <summary>
    /// Implements the notification logic for the related methods.
    /// </summary>
    /// <typeparam name = "TTask">The type of <see cref = "global::System.Threading.Tasks.Task"/> to set and monitor.</typeparam>
    /// <param name = "taskNotifier">The field notifier.</param>
    /// <param name = "newValue">The property's value after the change occurred.</param>
    /// <param name = "callback">(optional) A callback to invoke to update the property value.</param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    /// <returns><see langword="true"/> if the property was changed, <see langword="false"/> otherwise.</returns>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private bool SetPropertyAndNotifyOnCompletion<TTask>(ITaskNotifier<TTask> taskNotifier, TTask? newValue, global::System.Action<TTask?>? callback, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        where TTask : global::System.Threading.Tasks.Task
    {
        if (ReferenceEquals(taskNotifier.Task, newValue))
        {
            return false;
        }

        bool isAlreadyCompletedOrNull = newValue?.IsCompleted ?? true;
        taskNotifier.Task = newValue;
        OnPropertyChanged(propertyName);
        if (isAlreadyCompletedOrNull)
        {
            if (callback != null)
            {
                callback(newValue);
            }

            return true;
        }

        async void MonitorTask()
        {
            await __TaskExtensions.GetAwaitableWithoutEndValidation(newValue!);
            if (ReferenceEquals(taskNotifier.Task, newValue))
            {
                OnPropertyChanged(propertyName);
            }

            if (callback != null)
            {
                callback(newValue);
            }
        }

        MonitorTask();
        return true;
    }

    /// <summary>
    /// An interface for task notifiers of a specified type.
    /// </summary>
    /// <typeparam name = "TTask">The type of value to store.</typeparam>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    private interface ITaskNotifier<TTask>
        where TTask : global::System.Threading.Tasks.Task
    {
        /// <summary>
        /// Gets or sets the wrapped <typeparamref name = "TTask"/> value.
        /// </summary>
        TTask? Task { get; set; }
    }

    /// <summary>
    /// A wrapping class that can hold a <see cref = "global::System.Threading.Tasks.Task"/> value.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected sealed class TaskNotifier : ITaskNotifier<global::System.Threading.Tasks.Task>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref = "TaskNotifier"/> class.
        /// </summary>
        internal TaskNotifier()
        {
        }

        private global::System.Threading.Tasks.Task? task;
        /// <inheritdoc/>
        global::System.Threading.Tasks.Task? ITaskNotifier<global::System.Threading.Tasks.Task>.Task { get => this.task; set => this.task = value; }

        /// <summary>
        /// Unwraps the <see cref = "global::System.Threading.Tasks.Task"/> value stored in the current instance.
        /// </summary>
        /// <param name = "notifier">The input <see cref = "TaskNotifier{TTask}"/> instance.</param>
        public static implicit operator global::System.Threading.Tasks.Task?(TaskNotifier? notifier)
        {
            return notifier?.task;
        }
    }

    /// <summary>
    /// A wrapping class that can hold a <see cref = "global::System.Threading.Tasks.Task{T}"/> value.
    /// </summary>
    /// <typeparam name = "T">The type of value for the wrapped <see cref = "global::System.Threading.Tasks.Task{T}"/> instance.</typeparam>
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.INotifyPropertyChangedGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    protected sealed class TaskNotifier<T> : ITaskNotifier<global::System.Threading.Tasks.Task<T>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref = "TaskNotifier{TTask}"/> class.
        /// </summary>
        internal TaskNotifier()
        {
        }

        private global::System.Threading.Tasks.Task<T>? task;
        /// <inheritdoc/>
        global::System.Threading.Tasks.Task<T>? ITaskNotifier<global::System.Threading.Tasks.Task<T>>.Task { get => this.task; set => this.task = value; }

        /// <summary>
        /// Unwraps the <see cref = "global::System.Threading.Tasks.Task{T}"/> value stored in the current instance.
        /// </summary>
        /// <param name = "notifier">The input <see cref = "TaskNotifier{TTask}"/> instance.</param>
        public static implicit operator global::System.Threading.Tasks.Task<T>?(TaskNotifier<T>? notifier)
        {
            return notifier?.task;
        }
    }
    [global::System.CodeDom.Compiler.GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCode]
    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
    [global::System.Obsolete("This type is not intended to be used directly by user code")]
    internal static class __KnownINotifyPropertyChangedArgs
    {
        [global::System.Obsolete("This field is not intended to be referenced directly by user code")]
        public static readonly global::System.ComponentModel.PropertyChangedEventArgs Compositor = new global::System.ComponentModel.PropertyChangedEventArgs("Compositor");
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.Obsolete("This field is not intended to be referenced directly by user code")]
        public static readonly global::System.ComponentModel.PropertyChangedEventArgs TintOpacity = new global::System.ComponentModel.PropertyChangedEventArgs("TintOpacity");
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.Obsolete("This field is not intended to be referenced directly by user code")]
        public static readonly global::System.ComponentModel.PropertyChangedEventArgs LuminosityOpacity = new global::System.ComponentModel.PropertyChangedEventArgs("LuminosityOpacity");
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.Obsolete("This field is not intended to be referenced directly by user code")]
        public static readonly global::System.ComponentModel.PropertyChangedEventArgs TintColor = new global::System.ComponentModel.PropertyChangedEventArgs("TintColor");
        [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
        [global::System.Obsolete("This field is not intended to be referenced directly by user code")]
        public static readonly global::System.ComponentModel.PropertyChangedEventArgs LuminosityColor = new global::System.ComponentModel.PropertyChangedEventArgs("LuminosityColor");
    }
}

/// <summary>
/// An internal helper used to support <see cref="ObservableObject"/> and generated code from its template.
/// This type is not intended to be used directly by user code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("This type is not intended to be used directly by user code")]
public static class __TaskExtensions
{
    /// <summary>
    /// Gets an awaitable object that skips end validation.
    /// </summary>
    /// <param name="task">The input <see cref="Task"/> to get the awaitable for.</param>
    /// <returns>A <see cref="TaskAwaitableWithoutEndValidation"/> object wrapping <paramref name="task"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This method is not intended to be called directly by user code")]
    public static TaskAwaitableWithoutEndValidation GetAwaitableWithoutEndValidation(this Task task)
    {
        return new(task);
    }

    /// <summary>
    /// A custom task awaitable object that skips end validation.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This type is not intended to be called directly by user code")]
    public readonly struct TaskAwaitableWithoutEndValidation
    {
        /// <summary>
        /// The wrapped <see cref="Task"/> instance to create an awaiter for.
        /// </summary>
        private readonly Task task;

        /// <summary>
        /// Creates a new <see cref="TaskAwaitableWithoutEndValidation"/> instance with the specified parameters.
        /// </summary>
        /// <param name="task">The wrapped <see cref="Task"/> instance to create an awaiter for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TaskAwaitableWithoutEndValidation(Task task)
        {
            this.task = task;
        }

        /// <summary>
        /// Gets an <see cref="Awaiter"/> instance for the current underlying task.
        /// </summary>
        /// <returns>An <see cref="Awaiter"/> instance for the current underlying task.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Awaiter GetAwaiter()
        {
            return new(this.task);
        }

        /// <summary>
        /// An awaiter object for <see cref="TaskAwaitableWithoutEndValidation"/>.
        /// </summary>
        public readonly struct Awaiter : ICriticalNotifyCompletion
        {
            /// <summary>
            /// The underlying <see cref="TaskAwaiter"/> instance.
            /// </summary>
            private readonly TaskAwaiter taskAwaiter;

            /// <summary>
            /// Creates a new <see cref="Awaiter"/> instance with the specified parameters.
            /// </summary>
            /// <param name="task">The wrapped <see cref="Task"/> instance to create an awaiter for.</param>
            public Awaiter(Task task)
            {
                this.taskAwaiter = task.GetAwaiter();
            }

            /// <summary>
            /// Gets whether the operation has completed or not.
            /// </summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.taskAwaiter.IsCompleted;
            }

            /// <summary>
            /// Ends the await operation.
            /// </summary>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult()
            {
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnCompleted(Action continuation)
            {
                this.taskAwaiter.OnCompleted(continuation);
            }

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void UnsafeOnCompleted(Action continuation)
            {
                this.taskAwaiter.UnsafeOnCompleted(continuation);
            }
        }
    }
}

#pragma warning restore