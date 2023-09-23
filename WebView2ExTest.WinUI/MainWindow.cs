// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WebView2Ex;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WebView2ExTest.WinUI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : MicaWindow
{
    public MainWindow()
    {
        var wv2 = new WebView2ExSimplified() { InitialUri = "https://www.google.com" };
        Content = wv2;
        ExtendsContentIntoTitleBar = true;
        wv2.RuntimeInitialized += delegate
        {
            WebView2Runtime w = wv2.WebView2Runtime!;
            w.Controller!.DefaultBackgroundColor =
#if NonWinRTWebView2
            System.Drawing.Color.Transparent
#else
            Microsoft.UI.Colors.Transparent
#endif
            ;
            //await w.CoreWebView2.Profile.AddBrowserExtensionAsync(@"D:\ex\gmgoamodcdcjnbaobigkjelfplakmdhh\3.19_1");
        };
    }
}

// Simplified API
partial class WebView2ExSimplified : WebView2Ex.UI.WebView2Ex
{
    public WebView2ExSimplified()
    {
        Loaded += WebView2ExSimplified_Loaded;
    }

    private async void WebView2ExSimplified_Loaded(object sender, RoutedEventArgs e)
    {
        var window = AppWindow.GetFromWindowId(XamlRoot.ContentWindow.WindowId);
        SetWindow(window);
        // Assuming we create our own runtime
        WebView2Runtime = await WebView2Runtime.CreateAsync(
            await WebView2Environment.CreateAsync(),
            (nint)window.Id.Value
        );
        WebView2Runtime.CoreWebView2!.Navigate(InitialUri);
        RuntimeInitialized?.Invoke();
    }

    public event Action? RuntimeInitialized;
    // I'm lazy to implement normal observable property for Uri,
    // so I create one for initial Uri for simplicity.
    // You can technically do this by subscribing to WebView2Runtime.CoreWebView2.SourceChanged event
    [ObservableProperty]
    string _InitialUri = "about:blank";
}