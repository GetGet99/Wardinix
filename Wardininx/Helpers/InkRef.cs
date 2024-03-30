using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using Windows.Foundation;

namespace Wardininx;

public sealed class InkRefTracker : IDisposable
{
    private readonly List<InkRef> InkRefCollection = [];
    public InkRef GetRef(InkStroke InkStroke)
    {
        var inkref = InkRefCollection.FirstOrDefault(inkref => inkref.InkStroke == InkStroke);
        if (inkref is null)
        {
            inkref = new InkRef(InkStroke, this);
            InkRefCollection.Add(inkref);
        }
        else
        {
            inkref.AddUsageCount();
        }
        return inkref;
    }
    public InkRef[] GetRefs(IEnumerable<InkStroke> InkStrokes)
    {
        return (from x in InkStrokes select GetRef(x)).ToArray();
    }
    public void MarkUnused(InkRef inkRef)
    {
        inkRef.RemoveUseageCount();
        if (inkRef.UsageCount == 0) InkRefCollection.Remove(inkRef);
    }
    public void Dispose()
    {
        foreach (var a in InkRefCollection) a.Dispose();
    }
}
public sealed class InkRef(InkStroke InkStroke, InkRefTracker owner) : IDisposable
{
    readonly static InkStroke dummyInkStroke = new InkStrokeBuilder().CreateStroke([new Point()]);
    public InkStroke InkStroke { get; private set; } = InkStroke;
    public int UsageCount { get; private set; } = 1;
    public void AddUsageCount() => UsageCount++;
    public void RemoveUseageCount() => UsageCount--;

    public InkStroke CreateNew()
    {
        return InkStroke = InkStroke.Clone();
    }

    public void MarkUnused() => owner.MarkUnused(this);
    public void Dispose()
    {
        InkStroke = dummyInkStroke;
    }
}
