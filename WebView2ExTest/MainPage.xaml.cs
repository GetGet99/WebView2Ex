using CommunityToolkit.Mvvm.Input;
using WebView2Ex;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace WebView2ExTest;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        WebView2Ex1.SetWindow(CoreWindow.GetForCurrentThread());
        //WebView2Ex2.SetWindow(CoreWindow.GetForCurrentThread());
        InitializeAsync();
    }
    async void InitializeAsync()
    {
        WebView2Ex1.WebView2Runtime = await WebView2Runtime.CreateAsync();
        WebView2Ex1.WebView2Runtime.CoreWebView2.Navigate("https://www.discord.com/");
        //WebView2Ex2.WebView2Runtime = await WebView2Runtime.CreateAsync(WebView2Ex1.WebView2Runtime.Environment);
        //WebView2Ex2.WebView2Runtime.CoreWebView2.Navigate("https://www.google.com/");
    }
    [RelayCommand]
    void SwapRuntime()
    {
        //var (wv21, wv22)
        //    = (WebView2Ex1.WebView2Runtime, WebView2Ex2.WebView2Runtime);
        //WebView2Ex1.WebView2Runtime = wv22; // intentionally did not take off old owner
        //WebView2Ex2.WebView2Runtime = wv21;
    }
}
