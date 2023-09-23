#nullable enable
using System;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WinUI3
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
#endif
namespace WebView2Ex.UI;

partial class WebView2Ex
{
    long manipulationModeChangedToken, visibilityChangedToken;
    void RegisterEventsInit()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        manipulationModeChangedToken = RegisterPropertyChangedCallback(ManipulationModeProperty,
           OnManipulationModePropertyChanged);
        visibilityChangedToken = RegisterPropertyChangedCallback(VisibilityProperty,
           OnVisibilityPropertyChanged);
        OnVisibilityPropertyChanged(null, null);
        RegisterXamlEventHandlers();
        //RegisterDragDropEvents();
    }
    void UnregisterInitEvent()
    {
        if (manipulationModeChangedToken != 0)
        {
            UnregisterPropertyChangedCallback(ManipulationModeProperty, manipulationModeChangedToken);
            manipulationModeChangedToken = 0;
        }

        if (visibilityChangedToken != 0)
        {
            UnregisterPropertyChangedCallback(VisibilityProperty, visibilityChangedToken);
            visibilityChangedToken = 0;
        }
        UnregisterXamlEventHandlers();
        //UnregisterDragDropEvents();
    }
    void RegisterXamlEventHandlers()
    {
        GettingFocus += HandleGettingFocus;
        GotFocus += HandleGotFocus;
        RegisterXamlPointerEventHandlers();
        RegisterXamlKeyEventHandlers();

        SizeChanged += HandleSizeChanged;

    }
    void UnregisterXamlEventHandlers()
    {
        GettingFocus -= HandleGettingFocus;
        GotFocus -= HandleGotFocus;
        UnregisterXamlPointerEventHandlers();
        UnregisterXamlKeyEventHandlers();

        SizeChanged -= HandleSizeChanged;
    }

    void OnManipulationModePropertyChanged(DependencyObject? sender, DependencyProperty? dp)
        => Environment.FailFast("WebView2.ManipulationMode cannot be set to anything other than \"None\".");

    void RegisterEtc()
    {

    }
    void UnregisterEtcEvents()
    {
        var xamlRoot = XamlRoot;
        if (xamlRoot != null)
        {
            xamlRoot.Changed -= XamlRootChangedHanlder;
        }
#if WINDOWS_UWP
        Window.Current.VisibilityChanged -= VisiblityChangedHandler;
#elif WinUI3
        ParentWindow.Changed -= AppWindowChangedHandler;
#endif
        CompositionTarget.Rendered -= HandleRendered;
    }
}
