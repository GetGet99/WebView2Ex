// Original: https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/WebView2/WebView2.cpp
#if WINDOWS_UWP
using WebView2Ex.Natives;
using Windows.UI.Core;
using Windows.UI.WindowManagement;
using Windows.Win32.Foundation;
using static WebView2Ex.Natives.User32;
#elif WinUI3
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
#endif
namespace WebView2Ex.UI;

partial class WebView2Ex
{
    public partial void Dispose();
    public partial void Close();
#if WINDOWS_UWP
    public void SetWindow(HWND window)
    {
        ParentWindow = window;
        UpdateWindow();
    }
    public void SetWindow(AppWindow appWindow)
    {
        SetWindow((HWND)(nint)((IApplicationWindow_HwndInterop)(dynamic)appWindow).WindowHandle.Value);
    }
    public void SetWindow(CoreWindow coreWindow)
    {
        SetWindow(HWNDFromCoreWindow(coreWindow));
    }
#elif WinUI3
    public void SetWindow(AppWindow window)
    {
        ParentWindow = window;
        UpdateWindow();
    }
#endif
}
