using Wardininx.API;
using Windows.UI.Xaml;

namespace Wardininx.Controls;

public abstract class AbstractedUI
{
    UIElement _UI;
    public T UnsafeGetElement<T>(Document document) where T : UIElement => (T)(_UI ??= CreateUI(document));
    protected abstract UIElement CreateUI(Document document);
}