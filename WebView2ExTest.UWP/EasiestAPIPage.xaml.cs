﻿using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using WebView2Ex;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WebView2ExTest;

public sealed partial class EasiestAPIPage : Page
{
    public EasiestAPIPage()
    {
        InitializeComponent();
        // Before Runtime is initialized, it will be null
        wv2ex.RuntimeInitialized += RuntimeInitialized;
    }

    private void RuntimeInitialized()
    {
        // Do whatever with wv2ex.WebView2Runtime
        wv2ex.WebView2Runtime!.CompositionController!.DefaultBackgroundColor = Colors.Transparent;
    }
}
// Simplified API
partial class WebView2ExSimplified : WebView2Ex.UI.WebView2Ex
{
    public WebView2ExSimplified()
    {
        // Assuming we are on core window only
        SetWindow(CoreWindow.GetForCurrentThread());

        InitializeAsync();
    }
    async void InitializeAsync()
    {
        // Assuming we create our own runtime
        WebView2Runtime = await WebView2Runtime.CreateAsync();
        WebView2Runtime.CoreWebView2!.Navigate(InitialUri);
        RuntimeInitialized?.Invoke();
    }
    public event Action RuntimeInitialized;
    // I'm lazy to implement normal observable property for Uri,
    // so I create one for initial Uri for simplicity.
    // You can technically do this by subscribing to WebView2Runtime.CoreWebView2.SourceChanged event
    [ObservableProperty]
    string _InitialUri = "about:blank";
}
// Simplified API
partial class WebView2ExBasicMapping : WebView2Ex.UI.WebView2Ex
{
    public WebView2ExBasicMapping()
    {
        // Assuming we are on core window only
        SetWindow(CoreWindow.GetForCurrentThread());

        InitializeAsync();
    }
    
    // normal TaskCompletionSource does not exist in UWP
    TaskCompletionSource<bool> WebView2RuntimeTCS;
    async void InitializeAsync()
    {
        // Assuming we create our own runtime
        WebView2Runtime = await WebView2Runtime.CreateAsync();
        WebView2Runtime.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
        WebView2RuntimeTCS.SetResult(true);
        CoreWebView2Initialized?.Invoke(this, new());
        WebView2 wv;
    }

    private void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        NavigationCompleted?.Invoke(sender, args);
    }

    public TypedEventHandler<WebView2ExBasicMapping, EventArgs> CoreWebView2Initialized;
    public IAsyncAction EnsureCoreWebView2Async() => WebView2RuntimeTCS.Task.AsAsyncAction();
    public event TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs> NavigationCompleted;
}