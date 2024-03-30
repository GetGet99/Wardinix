using Windows.UI.Xaml;

namespace Wardininx.Controls;

public abstract class AbstractedUI
{
    UIElement _UI;
    public T UnsafeGetElement<T>() where T : UIElement => (T)(_UI ??= CreateUI());
    protected abstract UIElement CreateUI();
}