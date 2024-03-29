﻿// Original: https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/WebView2/WebView2.cpp
#nullable enable
#if WINDOWS_UWP
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using CompositionTarget = Windows.UI.Xaml.Media.CompositionTarget;
#elif WinUI3
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using CompositionTarget = Microsoft.UI.Xaml.Media.CompositionTarget;
#endif

namespace WebView2Ex.UI;

partial class WebView2Ex
{
    bool m_isVisible;
    bool m_renderedRegistered = false;

    void OnVisibilityPropertyChanged(DependencyObject? sender, DependencyProperty? dp)
    {
        UpdateRenderedSubscriptionAndVisibility();
    }

    void CheckAndUpdateVisibility(bool force = false)
    {
        // Keep booleans in this order to prevent doing expensive tree walk if we don't have to.
        bool currentVisibility = Visibility == Visibility.Visible &&
                                 IsLoaded &&
                                 isHostVisible &&
                                 AreAllAncestorsVisible();
        if (m_isVisible != currentVisibility || force)
        {
            m_isVisible = currentVisibility;
            UpdateCoreWebViewVisibility();
        }
    }

    // When we hide the CoreWebView too early, we would see a flash caused by SystemVisualBridge's BackgroundColor being displayed.
    // To resolve this, we delay the call to hide CoreWV if the WebView is being hidden.
    void UpdateCoreWebViewVisibility()
    {
        var Controller = this.Controller;
        void updateCoreWebViewVisibilityAction()
        {
            var Controller = this.Controller;
            if (Controller is not null) Controller.IsVisible = m_isVisible;
        }

        if (!m_isVisible && isHostVisible)
        {
            Utility.ScheduleActionAfterWait(Dispatcher, updateCoreWebViewVisibilityAction, 200);
        }
        else
        {
            if (Controller is not null)
                Controller.IsVisible = m_isVisible;
        }
    }

    bool AreAllAncestorsVisible()
    {
        bool allAncestorsVisible = true;
        DependencyObject? parentAsDO = Parent;
        while (parentAsDO is UIElement parentAsUIE)
        {
            Visibility parentVisibility = parentAsUIE.Visibility;
            if (parentVisibility == Visibility.Collapsed)
            {
                allAncestorsVisible = false;
                break;
            }
            parentAsDO = VisualTreeHelper.GetParent(parentAsDO);
        }

        return allAncestorsVisible;
    }
    void UpdateRenderedSubscriptionAndVisibility()
    {
        // The Rendered subscription is turned off for better performance when this element is hidden, or not loaded.
        // However, when this element is effectively hidden due to an ancestor being hidden, we should still subscribe --
        // otherwise, if the ancestor becomes visible again, we won't have the check in HandleRendered to inform us.
        if (IsLoaded && Visibility == Visibility.Visible)
        {
            if (!m_renderedRegistered)
            {
                CompositionTarget.Rendered += HandleRendered;
                m_renderedRegistered = true;
            }
        }
        else
        {
            CompositionTarget.Rendered -= HandleRendered;
            m_renderedRegistered = false;
        }
        CheckAndUpdateVisibility(true);
    }
}
