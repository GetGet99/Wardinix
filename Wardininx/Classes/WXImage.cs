#nullable enable
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Wardininx.Classes;

class WXImage
{
    MemoryStream Content;
    public static Task<WXImage?> FromClipboardAsync() => FromDataPackageViewAsync(Clipboard.GetContent());
    static async Task<T> GetAsAsync<T>(DataPackageView dataObject, string format) => (T)(await dataObject.GetDataAsync(format));
    public static async Task<WXImage?> FromDataPackageViewAsync(DataPackageView data)
    {
        if (data.Contains("PNG"))
        {
            var pngdata = (await GetAsAsync<IRandomAccessStream>(data, "PNG")).AsStream();
            var ms = new MemoryStream();
            await pngdata.CopyToAsync(ms);
            return new() { Content = ms };
        }
        else if (data.Contains("Bitmap"))
        {
            var getbmp = await data.GetBitmapAsync();
            var openread = await getbmp.OpenReadAsync();
            var bmpdata = openread.AsStream();
            var ms = new MemoryStream();
            await bmpdata.CopyToAsync(ms);
            return new() { Content = ms };
        }
        return null;
    }
    public async Task<LoadedImageSurface> GetImageSurfaceAsync()
    {
        var ms = new MemoryStream();
        Content.Position = 0;
        await Content.CopyToAsync(ms);
        ms.Position = 0;
        return LoadedImageSurface.StartLoadFromStream(ms.AsRandomAccessStream());
    }
    public async Task<BitmapImage> GetBitmapImageAsync()
    {
        BitmapImage bitmapSource = new();
        var ms = new MemoryStream();
        Content.Position = 0;
        await Content.CopyToAsync(ms);
        ms.Position = 0;
        await bitmapSource.SetSourceAsync(ms.AsRandomAccessStream());
        return bitmapSource;
    }
    public byte[] ToBytes()
    {
        var arr = Content.ToArray();
        Content.Position = 0;
        return arr;
    }
    public static WXImage FromBytes(byte[] bytes)
    {
        var ms = new MemoryStream(bytes);
        return new() { Content = ms };
    }
}