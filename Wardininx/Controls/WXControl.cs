using Windows.UI.Xaml.Controls;

namespace Wardininx.Controls;

class WXControl : Control
{
    public WXControl()
    {
        // Makes debugging easier since the name is shown in Visual Tree
        Name = GetType().Name;
        DefaultStyleKey = GetType();
    }
}