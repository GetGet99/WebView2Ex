// Original: https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/WebView2/WebView2.cpp
#nullable enable
#if NonWinRTWebView2
extern alias WV2;
using WV2::Microsoft.Web.WebView2.Core;
#else
using Microsoft.Web.WebView2.Core;
#endif
using Windows.UI.ViewManagement;
using Windows.UI.Composition;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Hosting;
#elif WinUI3
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Hosting;
using System.Runtime.InteropServices;
using System.Diagnostics;
using WinRT;
#endif
namespace WebView2Ex.UI;

partial class WebView2Ex
{

    async Task TryCompleteInitialization()
    {
        XamlRootChangedHelper(true);
        var xamlRoot = XamlRoot;
        if (xamlRoot != null)
        {
            xamlRoot.Changed += XamlRootChangedHanlder;
        }
        else
        {
#if WINDOWS_UWP
            Window.Current.VisibilityChanged += VisiblityChangedHandler;
#elif WinUI3
            ParentWindow.Changed += AppWindowChangedHandler;
#endif
        }

        // WebView2 in WinUI 2 is a ContentControl that either renders its web content to a SpriteVisual, or in the case that
        // the WebView2 Runtime is not installed, renders a message to that effect as its Content. In the case where the
        // WebView2 starts with Visibility.Collapsed, hit testing code has trouble seeing the WebView2 if it does not have
        // Content. To work around this, give the WebView2 a transparent Grid as Content that hit testing can find. The size
        // of this Grid must be kept in sync with the size of the WebView2 (see ResizeChildPanel()).
        var grid = new Grid { Background = new SolidColorBrush(Colors.Transparent) };
        Content = grid;
#if WINDOWS_UWP
        visual ??= ElementCompositionPreview.GetElementVisual(this).Compositor.CreateSpriteVisual();

        SetCoreWebViewAndVisualSize((float)ActualWidth, (float)ActualHeight);

        ElementCompositionPreview.SetElementChildVisual(this, visual);

        await Task.CompletedTask;
#elif WinUI3
        var (MUXVisual, WUXVisual) = await CreateVisual();
        visual ??= WUXVisual;
        this.MUXVisual = MUXVisual;

        SetCoreWebViewAndVisualSize((float)ActualWidth, (float)ActualHeight);

        ElementCompositionPreview.SetElementChildVisual(this, MUXVisual);

#endif

        var CompositionController = this.CompositionController;
        if (CompositionController is not null)
            CompositionController.RootVisualTarget = visual;
    }
    Visual? visual;
#if WinUI3
    Microsoft.UI.Composition.Visual? MUXVisual;
#endif
}