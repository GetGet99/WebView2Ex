// Original: https://github.com/microsoft/microsoft-ui-xaml/blob/main/dev/WebView2/WebView2.cpp
#nullable enable
#if NonWinRTWebView2
extern alias WV2;
using WV2::Microsoft.Web.WebView2.Core;
using WV2Rect = System.Drawing.Rectangle;
#else
using Microsoft.Web.WebView2.Core;
using WV2Rect = Windows.Foundation.Rect;
#endif
using System;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.Win32;
using Windows.Win32.Foundation;
using SysPoint = System.Drawing.Point;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Diagnostics;
using Point = Windows.Foundation.Point;
using WebView2Ex.Natives;
using static WebView2Ex.Natives.User32;
#if WinUI3
using WinRT.Interop;
#endif
namespace WebView2Ex.UI;

partial class WebView2Ex
{
    Point hostWindowPosition;
    Point webViewScaledPosition, webViewScaledSize;
    HWND m_tempHostHwnd, m_inputWindowHwnd;


    unsafe HWND EnsureTemporaryHostHwnd()
    {
        // If we don't know the parent yet, either use the CoreWindow as the parent,
        // or if we don't have one, create a dummy hwnd to be the temporary parent.
        // Using a dummy parent all the time won't work, since we can't reparent the
        // browser from a Non-ShellManaged Hwnd (dummy) to a ShellManaged one (CoreWindow).
#if WINDOWS_UWP
        if (ParentWindow != default) return ParentWindow;
        CoreWindow coreWindow = CoreWindow.GetForCurrentThread();
        if (coreWindow is not null)
        {
            var coreWindowInterop = (ICoreWindowInterop)((dynamic)coreWindow);
            m_tempHostHwnd = new(coreWindowInterop.WindowHandle);
        }
        else
#endif
        {
            // Register the window class.
            string CLASS_NAME = "WEBVIEW2_TEMP_PARENT";
            HINSTANCE hInstance = PInvoke.GetModuleHandle(default(PCWSTR));
            fixed (char* classNameAsChars = CLASS_NAME)
            {
                WNDCLASSW wc = new()
                {
                    lpfnWndProc = DefWindowProc,
                    hInstance = hInstance,
                    lpszClassName = new(classNameAsChars)
                };

                RegisterClass(in wc);

                m_tempHostHwnd = new(CreateWindowEx(
                    0,
                    CLASS_NAME,                                // Window class
                    "Webview2 Temporary Parent",               // Window text
                    WINDOW_STYLE.WS_OVERLAPPED,                // Window style
                    0, 0, 0, 0,
                    default,                                   // Parent window
                    default,                                   // Menu
                    new UnsafeSafeHandle(hInstance, false),    // Instance handle
                    default                                    // Additional application data
                ));
            }
        }
        return m_tempHostHwnd;
    }
    HWND GetHostHwnd()
    {
#if WINDOWS_UWP
        if (ParentWindow != default)
        	return ParentWindow;
        
        
        return HWNDFromCoreWindow(CoreWindow.GetForCurrentThread());
#elif WinUI3
        if (ParentWindow is not null)
            return new((nint)ParentWindow.Id.Value);
        else
            return default;
#endif
    }
    HWND GetActiveInputWindowHwnd()
    {
        var inputWindowHwnd = GetFocus();
        if (inputWindowHwnd == default)
        {
            throw new COMException("A COM error has occured", Marshal.GetLastWin32Error());
        }
        Debug.Assert(inputWindowHwnd != GetHostHwnd()); // Focused XAML host window cannot be set as input hwnd
        return inputWindowHwnd;
    }

    void CheckAndUpdateWebViewPosition()
    {
        var CompositionController = this.Controller;
        if (CompositionController is null) return;

        // Skip this work if WebView2 has just been removed from the tree - otherwise the CWV2.Bounds update could cause a flicker.
        //
        // After WebView2 is removed from the tree, this handler gets run one more time during the frame's render pass 
        // (WebView2::HandleRendered()). The removed element's ActualWidth or ActualHeight could now evaluate to zero 
        // (if Width or Height weren't explicitly set), causing 0-sized Bounds to get applied below and clear the web content, 
        // producing a flicker that last until DComp Commit for this frame is processed by the compositor.
        if (!IsLoaded) return;

        // Check if the position of the WebView2 within the window has changed
        bool changed = false;
        var transform = TransformToVisual(null);
        var topLeft = transform.TransformPoint(new Point(0, 0));

        var scaledTopLeftX = Math.Ceiling(topLeft.X * rasterizationScale);
        var scaledTopLeftY = Math.Ceiling(topLeft.Y * rasterizationScale);

        if (scaledTopLeftX != webViewScaledPosition.X || scaledTopLeftY != webViewScaledPosition.Y)
        {
            webViewScaledPosition.X = scaledTopLeftX;
            webViewScaledPosition.Y = scaledTopLeftY;
            changed = true;
        }

        var scaledSizeX = Math.Ceiling(ActualWidth * rasterizationScale);
        var scaledSizeY = Math.Ceiling(ActualHeight * rasterizationScale);
        if (scaledSizeX != webViewScaledSize.X || scaledSizeY != webViewScaledSize.Y)
        {
            webViewScaledSize.X = scaledSizeX;
            webViewScaledSize.Y = scaledSizeY;
            changed = true;
        }

        if (changed)
        {
            // We create the Bounds using X, Y, width, and height
            CompositionController.Bounds = new WV2Rect(
                (int)(webViewScaledPosition.X),
                (int)(webViewScaledPosition.Y),
                (int)(webViewScaledSize.X),
                (int)(webViewScaledSize.Y)
            );
        }
    }

    Rect GetBoundingRectangle()
    {
        return new Rect(
            (webViewScaledPosition.X),
        (webViewScaledPosition.Y),
        (webViewScaledSize.X),
        (webViewScaledSize.Y));
    }

    void CheckAndUpdateWindowPosition()
    {
        var hostWindow = GetHostHwnd();
        if (hostWindow == default)
        {
            return;
        }

        SysPoint windowPosition = new(0, 0);
        ClientToScreen(hostWindow, ref windowPosition);
        if (hostWindowPosition.X != windowPosition.X || hostWindowPosition.Y != windowPosition.Y)
        {
            hostWindowPosition.X = windowPosition.X;
            hostWindowPosition.Y = windowPosition.Y;
            var Controller = this.Controller;
            Controller?.NotifyParentWindowPositionChanged();
        }
    }
}
