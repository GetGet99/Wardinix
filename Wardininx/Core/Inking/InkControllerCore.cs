#nullable enable
using System.Collections;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace Wardininx.Core.Inking;

public class InkControllerCore
{
    public InkPresenter InkPresenter { get; }
    public InkRefTracker InkRefTracker { get; } = new();
    public CoreWetStrokeUpdateSource CoreWetStrokeUpdateSource { get; }
    readonly CoreInkPresenterHost Host = new();
    readonly ContainerVisual InternalVisual;
    readonly Compositor Compositor;
    public InkControllerCore() : this(Window.Current.Compositor)
    {
    }
    public InkControllerCore(Compositor compositor)
    {
        Compositor = Window.Current.Compositor;
        InkPresenter = Host.InkPresenter;
        CoreWetStrokeUpdateSource = CoreWetStrokeUpdateSource.Create(InkPresenter);
        InternalVisual = Compositor.CreateContainerVisual();
        InternalVisual.RelativeSizeAdjustment = new(1, 1);
        Host.RootVisual = InternalVisual;
        // Modify so that we can swap undomanager
        ToDo.NotImplemented();

    }
    ContainerVisual? _editingView;
    public event Action EditingViewChanged;
    public ContainerVisual? EditingView {
        get => _editingView;
        set
        {
            if (value == _editingView) return;
            if (_editingView is not null)
            {
                _editingView.Children.Remove(InternalVisual);
                _editingView.Children.InsertAtTop(GetRedirectVisual());
            }
            _editingView = value;
            if (value is not null)
            {
                FreeRedirectVisuals.Enqueue((RedirectVisual)value.Children.First());
                value.Children.RemoveAll();
                value.Children.InsertAtTop(GetRedirectVisual());
            }
            EditingViewChanged?.Invoke();
        }
    }
    readonly Queue<RedirectVisual> FreeRedirectVisuals = [];
    RedirectVisual GetRedirectVisual()
    {
        if (!FreeRedirectVisuals.TryDequeue(out var result))
        {
            result = Compositor.CreateRedirectVisual(InternalVisual);
            result.RelativeSizeAdjustment = new(1, 1);
        }
        return result;
    }
    public void AddHost(ContainerVisual visual)
    {
        visual.Children.InsertAtTop(GetRedirectVisual());
    }
}