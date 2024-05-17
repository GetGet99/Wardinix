using Get.Data.Collections;
using Get.Data.Collections.Implementation;
using System.Collections;
using System.Numerics;
using Wardininx.Core.Inking;
using Wardininx.UndoRedos;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;

namespace Wardininx.API.Inking;
public partial class InkController : IEditingSession<InkControllerCore>
{
    public InkSelection StrokesSelectedItems { get; }
    public InkRefTracker InkRefTracker => Core.InkRefTracker;
    public InkInputProcessingConfiguration InputProcessingConfiguration => InkPresenter.InputProcessingConfiguration;

    public InkController(Document document, InkControllerCore inkControllerCore)
    {
        OwnerEditingDocument = document;
        Core = inkControllerCore;
        // Modify so that we can swap undomanager
        inkControllerCore.EditingViewChanged += EditingViewChanged;
        StrokesSelectedItems = new(this, inkControllerCore);
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