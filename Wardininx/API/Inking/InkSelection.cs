using Get.Data.Collections.Implementation;
using Get.Data.Collections;
using Wardininx.Core.Inking;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace Wardininx.API.Inking;

public class InkSelection(InkController controller, InkControllerCore Core)
{
    List<InkStroke> selectedStrokes = [];

    public bool IsEmpty => selectedStrokes.Count is 0;

    public int Count => selectedStrokes.Count;

    public Rect SelectWithPolyLine(IEnumerable<Point> points)
    {
        var region = Core.InkPresenter.StrokeContainer.SelectWithPolyLine(points);
        ResetSelectionCache();
        return region;
    }

    public Rect SelectWithLine(Point from, Point to)
    {
        var region = Core.InkPresenter.StrokeContainer.SelectWithLine(from, to);
        ResetSelectionCache();
        return region;
    }
    void ResetSelectionCache()
    {
        selectedStrokes.Clear();
        foreach (var stroke in Core.InkPresenter.StrokeContainer.GetStrokes())
        {
            if (stroke.Selected) selectedStrokes.Add(stroke);
        }
    }

    public void Cut()
    {
        Copy();
        Delete();
    }
    public void Copy()
    {
        Core.InkPresenter.StrokeContainer.CopySelectedToClipboard();
    }
    public void Duplicate()
    {
        // Implement Undo/Redo system
        ToDo.NotImplemented();
        var oldSelectedStrokes = selectedStrokes;
        selectedStrokes = new(selectedStrokes.Count);
        ToDo.NotImplemented();
        foreach (var stroke in oldSelectedStrokes)
        {
            var newStroke = stroke.Clone();
            selectedStrokes.Add(newStroke);
            newStroke.Selected = true;
            stroke.Selected = false;
        }
        Core.InkPresenter.StrokeContainer.AddStrokes(selectedStrokes);
    }

    public void Clear()
    {
        foreach (var s in selectedStrokes)
        {
            s.Selected = false;
        }
        selectedStrokes.Clear();
    }
    public void Delete()
    {
        // Implement Undo/Redo system
        ToDo.NotImplemented();
        Core.InkPresenter.StrokeContainer.DeleteSelected();
        selectedStrokes.Clear();
    }

    public void Insert(int index, InkStroke item)
    {
        selectedStrokes.Insert(index, item);
        item.Selected = true;
    }

    public void RemoveAt(int index)
    {
        selectedStrokes[index].Selected = false;
        selectedStrokes.RemoveAt(index);
    }
}