using Wardininx.Core.Inking;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;

namespace Wardininx.API.Inking;
public readonly partial struct InkController : IEditingSession<InkControllerCore>
{
    Document OwnerEditingDocument { get; }
    public InkSelection StrokesSelectedItems { get; }
    readonly InkControllerCore Core;
    public InkRefTracker InkRefTracker => Core.InkRefTracker;
    public InkInputProcessingConfiguration InputProcessingConfiguration => InkPresenter.InputProcessingConfiguration;

    public InkController(Document document, InkControllerCore inkControllerCore)
    {
        OwnerEditingDocument = document;
        Core = inkControllerCore;
        // Modify so that we can swap undomanager
        inkControllerCore.EditingViewChanged += EditingViewChanged;
        StrokesSelectedItems = new(document, inkControllerCore);
    }

    private void WetUpdateSourceEvent(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
    {
        Snap(args.NewInkPoints);
    }


    void Snap(IList<InkPoint> points)
    {
        //for (int i = 0; i < points.Count; i++)
        //{
        //    points[i] = new(points[i].Position with { X = WXInkCanvas.RealCanvasSize / 2 }, points[i].Pressure);
        //}
    }
}