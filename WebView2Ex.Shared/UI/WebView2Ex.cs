// Original: https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/WebView2/WebView2.cpp
#nullable enable
#if NonWinRTWebView2
extern alias WV2;
using WV2::Microsoft.Web.WebView2.Core;
#else
using Microsoft.Web.WebView2.Core;
#endif
using CommunityToolkit.Mvvm.ComponentModel;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#elif WinUI3
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
#endif
using Windows.Win32.Foundation;

namespace WebView2Ex.UI;

[ObservableObject]
public partial class WebView2Ex
    : UserControl
{
    [ObservableProperty]
    WebView2Runtime? _WebView2Runtime;
    CoreWebView2? CoreWebView2 => WebView2Runtime?.CoreWebView2;
    CoreWebView2CompositionController? CompositionController
        => WebView2Runtime?.CompositionController;
    CoreWebView2Controller? Controller
        => WebView2Runtime?.Controller;
    public WebView2Ex()
    {
        SetupSmoothScroll();
        ManipulationMode = ManipulationModes.None;
        
        RegisterEventsInit();
        IsTabStop = true;
        // Set the background for WebView2 to ensure it will be visible to hit-testing.
        //Background = new SolidColorBrush(Colors.Transparent);
    }
#if WINDOWS_UWP
    private HWND ParentWindow;
#elif WinUI3
    private AppWindow? ParentWindow;
#endif

    partial void OnWebView2RuntimeChanging(WebView2Runtime? value)
    {
        var oldRuntime = WebView2Runtime;
        if (oldRuntime is null) return;
#if WinUI3
        if (oldRuntime.RootVisualTarget == visual)
#else
        if (oldRuntime.RootVisualTarget == visual)
#endif
        {
            oldRuntime.RootVisualTarget = null;
            if (oldRuntime.Controller is not null)
                oldRuntime.Controller.IsVisible = false;
        }
        // oldRuntime.SetWindow(HWND.Null);
        oldRuntime.Owner = null;
        if (oldRuntime.CompositionController is not null)
            oldRuntime.CompositionController.CursorChanged -= CoreWebView2CursorChanged;
    }
    partial void OnWebView2RuntimeChanged(WebView2Runtime? value)
    {
        var newRuntime = WebView2Runtime;
        if (newRuntime is null) return;
        if (newRuntime.Owner is not null && newRuntime.Owner.WebView2Runtime == newRuntime)
            newRuntime.Owner.WebView2Runtime = null;
        UpdateWindow();
        if (newRuntime.CompositionController is not null)
            newRuntime.CompositionController.CursorChanged += CoreWebView2CursorChanged;
        if (newRuntime.Controller is not null)
            newRuntime.Controller.IsVisible = m_isVisible;
        if (visual is not null) newRuntime.RootVisualTarget = visual;
        UpdateSize();
    }
    void UpdateWindow()
    {
        WebView2Runtime?.SetWindow(ParentWindow);
    }
}