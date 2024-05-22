using Wardininx.API;
using Windows.UI.Xaml;

namespace Wardininx.Controls;

public abstract class AbstractedUIDocument
{
    UIElement _UI;
    public T UnsafeGetElement<T>(Document document) where T : UIElement => (T)(_UI ??= CreateUI(document));
    protected abstract UIElement CreateUI(Document document);
}
public abstract class AbstractedUI
{
    UIElement _UI;
    public T UnsafeGetElement<T>() where T : UIElement => (T)(_UI ??= CreateUI());
    protected abstract UIElement CreateUI();
}