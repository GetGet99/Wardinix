using Get.Data.Properties;
using System.Numerics;
using System.Runtime.CompilerServices;
using Wardininx.API;
using Wardininx.Core.Layers;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Wardininx.Controls.Layers;
abstract partial class LayerControl : WXControl
{
    public LayerControl(LayerCore abstracted)
    {
        abstracted.IsSelectedProperty.ValueChanged += (_, @new) => IsHitTestVisible = @new;
        IsHitTestVisible = abstracted.IsSelectedProperty.Value;
    }
}