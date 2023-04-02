using WebView2Ex;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace WebView2ExTest;

public sealed partial class PortableRuntimeAndSupportAppWindowPage : Page
{
    public PortableRuntimeAndSupportAppWindowPage()
    {
        InitializeComponent();

        // Set Window, accepts both CoreWindow and AppWindow instance
        wv2ex.SetWindow(CoreWindow.GetForCurrentThread());
        
        InitializeAsync();
    }
    async void InitializeAsync()
    {
        var environment =
            await WebView2Environment.CreateAsync();
            // similar to as CoreWebView2Environment.CreateAsync() but it adds default language

        wv2ex.WebView2Runtime = await WebView2Runtime.CreateAsync(environment);

        // WebView2Runtime reference
        // WebView2Runtime.CompositionController
        // WebView2Runtime.CoreWebView2
        // WebView2Runtime.Environment

        // example usage
        wv2ex.WebView2Runtime.CompositionController.DefaultBackgroundColor = Colors.Transparent;


        wv2ex.WebView2Runtime.CoreWebView2.Navigate("https://www.google.com/");
    }
}
