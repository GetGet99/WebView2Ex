#nullable enable
#if NonWinRTWebView2
extern alias WV2;
using WV2::Microsoft.Web.WebView2.Core;
#else
using Microsoft.Web.WebView2.Core;
#endif

using System;
using System.Threading.Tasks;
using WebView2Ex.Natives;
using Windows.Globalization;
using Windows.UI.Core;
using Windows.UI.WindowManagement;
using Windows.Win32.Foundation;
using Windows.UI.Composition;
#if WinUI3
using System.Collections.Generic;
using System.Reflection;
using Microsoft.UI.Xaml;
using WinRT.Interop;
#endif

namespace WebView2Ex;

public static class WebView2Environment
{
    public static async ValueTask<CoreWebView2Environment> CreateAsync(CoreWebView2EnvironmentOptions? options = null)
    {
        string browserInstall = "";
        string userDataFolder = "";
        if (options is null)
        {
            options = new CoreWebView2EnvironmentOptions();
            var applicationLanguagesList = ApplicationLanguages.Languages;
            if (applicationLanguagesList.Count > 0)
            {
                options.Language = applicationLanguagesList[0];
            }
        }
#if !NonWinRTWebView2
        return await CoreWebView2Environment.CreateWithOptionsAsync(
            browserInstall,
            userDataFolder,
            options
        );
#else
        return await CoreWebView2Environment.CreateAsync(
            browserInstall,
            userDataFolder,
            options
        );
#endif
    }
}
public class WebView2Runtime : IDisposable
{
    public CoreWebView2CompositionController? CompositionController { get; private set; }
    public CoreWebView2Controller? Controller { get; private set; }
    public CoreWebView2? CoreWebView2 { get; private set; }
    public CoreWebView2Environment? Environment { get; private set; }
    internal UI.WebView2Ex? Owner;
//#if NonWinRTWebView2
//    Windows.Win32.Graphics.DirectComposition.IDCompositionVisual? buffer;
//    internal Windows.Win32.Graphics.DirectComposition.IDCompositionVisual? RootVisualTarget
//    {
//        get => buffer;
//        set
//        {
//            if (CompositionController is not null)
//                CompositionController.RootVisualTarget = buffer = value;
//        }
//    }
//#else
    Visual? buffer;
    internal Visual? RootVisualTarget
    {
        get => buffer;
        set
        {
            if (CompositionController is not null)
                CompositionController.RootVisualTarget = buffer = value;
        }
    }
//#endif
    private WebView2Runtime(
        CoreWebView2CompositionController CompositionController)
    {
        this.CompositionController = CompositionController;
#if !NonWinRTWebView2
        Controller = CompositionController;
#else
        Controller = GetController(CompositionController);
#endif

        Controller.ShouldDetectMonitorScaleChanges = false;
        CoreWebView2 = Controller.CoreWebView2;
        Environment = CoreWebView2.Environment;
    }
    public async static Task<WebView2Runtime> CreateAsync(CoreWebView2Environment env, CoreWebView2ControllerOptions options = null)
    {
#if !NonWinRTWebView2
        var windowRef = CoreWebView2ControllerWindowReference.CreateFromCoreWindow(CoreWindow.GetForCurrentThread());
#else
        var windowRef = default(HWND);
#endif
        var controller =
            options is null ? await env.CreateCoreWebView2CompositionControllerAsync(windowRef) :
            await env.CreateCoreWebView2CompositionControllerAsync(windowRef, options);
        return new(controller);
    }
    public async static Task<WebView2Runtime> CreateAsync(CoreWebView2Environment env, IntPtr windowHWND, CoreWebView2ControllerOptions options = null)
    {
#if WINDOWS_UWP
        var windowRef = CoreWebView2ControllerWindowReference.CreateFromCoreWindow(CoreWindow.GetForCurrentThread());
#elif NonWinRTWebView2
        var windowRef = new HWND(windowHWND);
#else
        var windowRef = CoreWebView2ControllerWindowReference.CreateFromWindowHandle((ulong)windowHWND);
#endif
        var controller =
            options is null ? await env.CreateCoreWebView2CompositionControllerAsync(windowRef) :
            await env.CreateCoreWebView2CompositionControllerAsync(windowRef, options);
        return new(controller);
    }
    public async static Task<WebView2Runtime> CreateAsync()
        => await CreateAsync(await WebView2Environment.CreateAsync(null));

    internal void SetWindow(HWND window)
    {
        return;
        if (Controller is not null)
#if !NonWinRTWebView2
            Controller.ParentWindow = CoreWebView2ControllerWindowReference.CreateFromWindowHandle((ulong)window.Value);
#else
            Controller.ParentWindow = window;
#endif
    }
#if WINDOWS_UWP
    internal void SetWindow(AppWindow appWindow)
    {
        var interop = (IApplicationWindow_HwndInterop)(dynamic)appWindow;
        SetWindow((HWND)(nint)interop.WindowHandle.Value);
    }
    internal void SetWindow(CoreWindow coreWindow)
    {
        if (Controller is not null)
            Controller.ParentWindow = CoreWebView2ControllerWindowReference.CreateFromCoreWindow(coreWindow);
    }
#elif WinUI3
    internal void SetWindow(Window XAMLWindow)
    {
        SetWindow(new HWND(WindowNative.GetWindowHandle(XAMLWindow)));
    }
    internal void SetWindow(Microsoft.UI.Windowing.AppWindow appWindow)
    {
        SetWindow(new HWND((nint)appWindow.Id.Value));
    }
#endif
    public void Dispose()
    {
        Controller?.Close();
        CompositionController = null;
        Controller = null;
        Environment = null;
        CoreWebView2 = null;
        GC.SuppressFinalize(this);
    }

#if NonWinRTWebView2
    readonly static FieldInfo RawNativeCompControllerField;
    readonly static ConstructorInfo ControllerConstructor;
    static WebView2Runtime()
    {
        var field = typeof(CoreWebView2CompositionController).GetField("_rawNative", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        if (field is null) throw new KeyNotFoundException();
        RawNativeCompControllerField = field;
        ControllerConstructor = typeof(CoreWebView2Controller).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
    }
    private static CoreWebView2Controller GetController(CoreWebView2CompositionController compcontroller)
    {
        var rawNativeobj = RawNativeCompControllerField.GetValue(compcontroller);
        if (rawNativeobj is null) throw new InvalidOperationException();
        return (CoreWebView2Controller)ControllerConstructor.Invoke(new object[] { rawNativeobj });
    }
#endif
}
