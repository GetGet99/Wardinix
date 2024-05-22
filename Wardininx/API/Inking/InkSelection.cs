using Wardininx.Core.Inking;
using Windows.Foundation;
using Get.Data.Collections.Linq;
namespace Wardininx.API.Inking;

public readonly struct InkSelection(Document document, InkControllerCore Core)
{
    public bool IsEmpty => Core.Selection.IsEmpty;

    public int Count => Core.Selection.Count;

    public Rect SelectWithPolyLine(IEnumerable<Point> points)
        => Core.Selection.SelectWithPolyLine(points);

    public Rect SelectWithLine(Point from, Point to)
        => Core.Selection.SelectWithLine(from, to);
    public void Cut()
    {
        Copy();
        DeleteSelected();
    }
    public void Copy()
        => Core.Selection.Copy();
    public void Duplicate()
    {
        var newStrokes = Core.Selection.Duplicate();
        var newStrokesRef = Core.InkRefTracker.GetRefs(newStrokes);
        var self = this;
        var c = Core;
        document.UndoManager.AddAction(new(
            "InkLayer.Selection.Duplicate()",
            Undo: delegate
            {
                // clear selection
                self.ClearSelection();
                foreach (var inkRef in newStrokesRef)
                {
                    inkRef.InkStroke.Selected = true;
                    // create new, and deselect the copy
                    inkRef.CreateNew().Selected = false;
                }
                c.InkPresenter.StrokeContainer.DeleteSelected();
            },
            Redo: delegate
            {
                self.ClearSelection();
                foreach (var inkRef in newStrokesRef)
                {
                    c.InkPresenter.StrokeContainer.AddStroke(inkRef.InkStroke);
                }
                c.InkPresenter.StrokeContainer.DeleteSelected();
            },
            Cleanup: delegate { }
        ));
    }
    public void ClearSelection() => Core.Selection.Clear();
    public void DeleteSelected()
    {
        // Does not restore back 100% (ordering is not right because there is no Insert() command for strokecontainer).
        ToDo.Note();
        var strokesRef = Core.InkRefTracker.GetRefs(Core.Selection.AsEnumerable());
        var self = this;
        var c = Core;
        document.UndoManager.DoAndAddAction(new(
            "InkLayer.Selection.DeleteSelected()",
            Redo: delegate
            {
                // clear selection
                self.ClearSelection();
                foreach (var inkRef in strokesRef)
                {
                    inkRef.InkStroke.Selected = true;
                    // create new, and deselect the copy
                    inkRef.CreateNew().Selected = false;
                }
                c.InkPresenter.StrokeContainer.DeleteSelected();
            },
            Undo: delegate
            {
                self.ClearSelection();
                foreach (var inkRef in strokesRef)
                {
                    c.InkPresenter.StrokeContainer.AddStroke(inkRef.InkStroke);
                }
                c.InkPresenter.StrokeContainer.DeleteSelected();
            },
            Cleanup: delegate { }
        ));
    }
}