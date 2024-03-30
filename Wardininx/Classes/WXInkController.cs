using System.Collections;
using Wardininx.Controls.Canvas;
using Wardininx.UndoRedos;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace Wardininx.Classes;
class WXInkController
{
    public WXInkSelectionController Selection { get; }
    public UndoManager UndoManager { get; }
    InkPresenter InkPresenter { get; }
    public InkRefTracker InkRefTracker { get; } = new();
    public InkInputProcessingConfiguration InputProcessingConfiguration => InkPresenter.InputProcessingConfiguration;
    public WXInkController(UndoManager UndoManager, InkPresenter InkPresenter)
    {
        this.UndoManager = UndoManager;
        this.InkPresenter = InkPresenter;
        Selection = new(this, InkPresenter);
        InkPresenter.StrokesCollected += (o, e) => RecordAddStrokes(e.Strokes);
        InkPresenter.StrokesErased += (o, e) => RecordEraseStrokes(e.Strokes);
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
        UndoManager.AddAction(toRet = new UndoableAction<(WXInkController, InkRef[])>("Erase", (this, _strokes), UndoParam:
            static x =>
            {
                var (@this, strokes) = x;
                @this.Selection.Clear();
                foreach (var ink in strokes)
                {
                    ink.InkStroke.Selected = true;
                    ink.CreateNew();
                }
                @this.InkPresenter.StrokeContainer.DeleteSelected();
            }, RedoParam: static x =>
            {
                var (@this, strokes) = x;
                @this.Selection.Clear();
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
        UndoManager.AddAction(toRet = new UndoableAction<(WXInkController, InkRef[])>("Erase", (this, _strokes), UndoParam:
            static x =>
            {
                var (@this, strokes) = x;
                @this.Selection.Clear();
                @this.InkPresenter.StrokeContainer.AddStrokes((from ink in strokes select ink.CreateNew()).ToArray());
            }, RedoParam: static x =>
            {
                var (@this, strokes) = x;
                @this.Selection.Clear();
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
class WXInkSelectionController(WXInkController controller, InkPresenter InkPresenter) : IList<InkStroke>
{
    List<InkStroke> selectedStrokes = [];

    public InkStroke this[int index]
    {
        get => selectedStrokes[index];
        set
        {
            var stroke = selectedStrokes[index];
            stroke.Selected = false;
            selectedStrokes[index] = value;
            value.Selected = true;
        }
    }

    public bool IsEmpty => selectedStrokes.Count is 0;

    public int Count => selectedStrokes.Count;

    bool ICollection<InkStroke>.IsReadOnly => false;

    public Rect SelectWithPolyLine(IEnumerable<Point> points)
    {
        var region = InkPresenter.StrokeContainer.SelectWithPolyLine(points);
        ResetSelectionCache();
        return region;
    }

    public Rect SelectWithLine(Point from, Point to)
    {
        var region = InkPresenter.StrokeContainer.SelectWithLine(from, to);
        ResetSelectionCache();
        return region;
    }
    void ResetSelectionCache()
    {
        selectedStrokes.Clear();
        foreach (var stroke in InkPresenter.StrokeContainer.GetStrokes())
        {
            if (stroke.Selected) selectedStrokes.Add(stroke);
        }
    }

    public void Cut()
    {
        Copy();
        InkPresenter.StrokeContainer.DeleteSelected();
    }
    public void Copy()
    {
        InkPresenter.StrokeContainer.CopySelectedToClipboard();
    }
    public void Duplicate()
    {
        var oldSelectedStrokes = selectedStrokes;
        selectedStrokes = new(selectedStrokes.Count);
        foreach (var stroke in oldSelectedStrokes)
        {
            var newStroke = stroke.Clone();
            selectedStrokes.Add(newStroke);
            newStroke.Selected = true;
            stroke.Selected = false;
        }
        InkPresenter.StrokeContainer.AddStrokes(selectedStrokes);
    }

    public void Add(InkStroke item)
    {
        selectedStrokes.Add(item);
        item.Selected = true;
    }

    public void Clear()
    {
        foreach (var s in selectedStrokes)
        {
            s.Selected = false;
        }
        selectedStrokes.Clear();
    }

    public bool Contains(InkStroke item)
    {
        return selectedStrokes.Contains(item);
    }

    public void CopyTo(InkStroke[] array, int arrayIndex)
    {
        selectedStrokes.CopyTo(array, arrayIndex);
    }

    public IEnumerator<InkStroke> GetEnumerator() => selectedStrokes.GetEnumerator();

    public int IndexOf(InkStroke item)
    {
        return selectedStrokes.IndexOf(item);
    }

    public void Insert(int index, InkStroke item)
    {
        selectedStrokes.Insert(index, item);
        item.Selected = true;
    }

    public bool Remove(InkStroke item)
    {
        if (selectedStrokes.Remove(item))
        {
            item.Selected = false;
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        selectedStrokes[index].Selected = false;
        selectedStrokes.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator() => selectedStrokes.GetEnumerator();
}