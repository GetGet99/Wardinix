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
    Document OwnerEditingDocument { get; }
    UndoManager UndoManager => OwnerEditingDocument.UndoManager;
    InkPresenter InkPresenter => Core.InkPresenter;
    InkControllerCore IEditingSession<InkControllerCore>.Core => Core;
    readonly InkControllerCore Core;

    private void EditingViewChanged()
    {
        if (Core.EditingView == ToDo.NotImplemented<Visual>())
        {
            InkPresenter.StrokesCollected += StrokesCollected;
            InkPresenter.StrokesErased += StrokesErased;
            Core.CoreWetStrokeUpdateSource.WetStrokeStarting += WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeContinuing += WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeCompleted += WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeStopping += WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeCompleted += WetUpdateSourceEvent;
        } else
        {
            InkPresenter.StrokesCollected -= StrokesCollected;
            InkPresenter.StrokesErased -= StrokesErased;
            Core.CoreWetStrokeUpdateSource.WetStrokeStarting -= WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeContinuing -= WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeCompleted -= WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeStopping -= WetUpdateSourceEvent;
            Core.CoreWetStrokeUpdateSource.WetStrokeCompleted -= WetUpdateSourceEvent;
        }
    }


    private void StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
    {
        RecordAddStrokes(args.Strokes);
    }
    private void StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
    {
        RecordEraseStrokes(args.Strokes);
    }
    public void UpdateDefaultDrawingAttributes(InkDrawingAttributes drawingAttributes)
        => InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);

    public UndoableAction AddStrokesAndRecord(IEnumerable<InkStroke> strokesToAdd)
    {
        var act = RecordAddStrokes(strokesToAdd);
        act.Redo();
        return act;
    }
    public UndoableAction EraseStrokesAndRecord(IEnumerable<InkStroke> strokesToErase)
    {
        var act = RecordEraseStrokes(strokesToErase);
        act.Redo();
        return act;
    }
    public UndoableAction RecordAddStrokes(IEnumerable<InkStroke> strokesAdded)
    {
        UndoableAction toRet;
        var _strokes = strokesAdded.Select(InkRefTracker.GetRef).ToArray();
        UndoManager.AddAction(toRet = new UndoableAction<(InkController, InkRef[])>("Erase", (this, _strokes), UndoParam:
            static x =>
            {
                var (@this, strokes) = x;
                @this.StrokesSelectedItems.Clear();
                foreach (var ink in strokes)
                {
                    ink.InkStroke.Selected = true;
                    ink.CreateNew();
                }
                @this.InkPresenter.StrokeContainer.DeleteSelected();
            }, RedoParam: static x =>
            {
                var (@this, strokes) = x;
                @this.StrokesSelectedItems.Clear();
                @this.InkPresenter.StrokeContainer.AddStrokes((from ink in strokes select ink.CreateNew()).ToArray());
            }, CleanupParam: (x, wasActionDone) =>
            {
                var (@this, strokes) = x;
                foreach (var stroke in strokes) stroke.Dispose();
            }
        ));
        return toRet;
    }
    public UndoableAction RecordEraseStrokes(IEnumerable<InkStroke> strokesErased)
    {
        UndoableAction toRet;
        var _strokes = strokesErased.Select(InkRefTracker.GetRef).ToArray();
        UndoManager.AddAction(toRet = new UndoableAction<(InkController, InkRef[])>("Erase", (this, _strokes), UndoParam:
            static x =>
            {
                var (@this, strokes) = x;
                @this.StrokesSelectedItems.Clear();
                @this.InkPresenter.StrokeContainer.AddStrokes((from ink in strokes select ink.CreateNew()).ToArray());
            }, RedoParam: static x =>
            {
                var (@this, strokes) = x;
                @this.StrokesSelectedItems.Clear();
                foreach (var ink in strokes)
                {
                    ink.InkStroke.Selected = true;
                    ink.CreateNew();
                }
                @this.InkPresenter.StrokeContainer.DeleteSelected();
            }, CleanupParam: (x, wasActionDone) =>
            {
                var (@this, strokes) = x;
                foreach (var stroke in strokes) stroke.Dispose();
            }
        ));
        return toRet;
    }
}