using Get.Data.Collections;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace Wardininx.Core.Inking;

public class InkSelectionCore(InkControllerCore Core) : IGDReadOnlyCollection<InkStroke>
{
    List<InkStroke> selectedStrokes = [];

    public bool IsEmpty => selectedStrokes.Count is 0;

    public int Count => selectedStrokes.Count;

    public InkStroke this[int index] => selectedStrokes[index];

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
    public void ResetSelectionCache()
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
    public IList<InkStroke> Duplicate()
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
        Core.InkPresenter.StrokeContainer.AddStrokes(selectedStrokes);
        return selectedStrokes;
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
        Core.InkPresenter.StrokeContainer.DeleteSelected();
        selectedStrokes.Clear();
    }
}