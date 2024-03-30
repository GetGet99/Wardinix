using System.Numerics;

using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml;
namespace Wardininx;
class ElementInteractionTracker : IInteractionTrackerOwner
{
    public bool IsInertiaOrInteracting { get; private set; }
    public InteractionTracker InteractionTracker { get; }
    public VisualInteractionSource ScrollPresenterVisualInteractionSource { get; }
    public ElementInteractionTracker(UIElement element)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        InteractionTracker = InteractionTracker.CreateWithOwner(visual.Compositor, this);
        //InteractionTracker.MinPosition = new Vector3(0, 0, 0);
        //InteractionTracker.MaxPosition = new Vector3(WXInkCanvas.RealCanvasSize, WXInkCanvas.RealCanvasSize, 0);
        //InteractionTracker.TryUpdatePosition(new(WXInkCanvas.RealCanvasSize / 2, WXInkCanvas.RealCanvasSize / 2, 0));
        //InteractionTracker.MinScale = 0.5f;
        //InteractionTracker.MaxScale = 3f;

        InteractionTracker.InteractionSources.Add(
            ScrollPresenterVisualInteractionSource = VisualInteractionSource.Create(visual)
        );
        ScrollPresenterVisualInteractionSource.IsPositionXRailsEnabled =
            ScrollPresenterVisualInteractionSource.IsPositionYRailsEnabled = true;


        ScrollPresenterVisualInteractionSource.PointerWheelConfig.PositionXSourceMode =
            ScrollPresenterVisualInteractionSource.PointerWheelConfig.PositionYSourceMode
            = InteractionSourceRedirectionMode.Enabled;

        ScrollPresenterVisualInteractionSource.PositionXChainingMode =
            ScrollPresenterVisualInteractionSource.ScaleChainingMode =
            InteractionChainingMode.Auto;

        ScrollPresenterVisualInteractionSource.PositionXSourceMode =
            ScrollPresenterVisualInteractionSource.PositionYSourceMode =
            ScrollPresenterVisualInteractionSource.ScaleSourceMode =
            InteractionSourceMode.EnabledWithInertia;

    }
    public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args)
    {

    }

    public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
    {
        IsInertiaOrInteracting = false;
    }

    public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
    {
        IsInertiaOrInteracting = true;
    }

    public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
    {
        IsInertiaOrInteracting = true;
    }

    public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args)
    {

    }
    Vector3 Vec = new(1000, 1000, 1000);
    public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
    {
        //InteractionTracker.MinPosition = args.Position - Vec;
        //InteractionTracker.MaxPosition = args.Position + Vec;
        ValuesChangedEvent?.Invoke(args);
    }
    public event Action<InteractionTrackerValuesChangedArgs>? ValuesChangedEvent;
}