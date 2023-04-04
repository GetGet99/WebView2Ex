#nullable enable
// Reference: winui3gallery://item/SystemBackdrops
using System.Runtime.InteropServices;
using WinRT;
using Microsoft.UI.Xaml;
using Microsoft.UI.Composition.SystemBackdrops;
using Windows.System;

namespace WebView2ExTest.WinUI;

public partial class MicaWindow : Window
{
    readonly static bool IsMicaInfinite = true;
    MicaController? m_micaController;
    SystemBackdropConfiguration? m_configurationSource;

    public MicaWindow()
    {
        TrySetMicaBackdrop();
    }
    bool TrySetMicaBackdrop()
    {
        if (MicaController.IsSupported())
        {
            WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

            // Hooking up the policy object
            m_configurationSource = new SystemBackdropConfiguration();


            // Initial configuration state.
            m_configurationSource.IsInputActive = true;

            m_micaController = new MicaController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_micaController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_micaController.SetSystemBackdropConfiguration(m_configurationSource);

            Activated += OnActivatedChange;
            Closed += OnWindowClosed;
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }

    private void OnActivatedChange(object _1, WindowActivatedEventArgs args)
    {
        if (m_configurationSource == null) return;
        bool IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        if (IsInputActive)
            m_configurationSource.IsInputActive = true;
        else if (!IsMicaInfinite)
            m_configurationSource.IsInputActive = false;
    }

    private void OnWindowClosed(object _1, WindowEventArgs _2)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        Activated -= OnActivatedChange;
        m_configurationSource = null;
    }
}

public static class WindowsSystemDispatcherQueueHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

    static object? m_dispatcherQueueController = null;
    public static void EnsureWindowsSystemDispatcherQueueController()
    {
        if (DispatcherQueue.GetForCurrentThread() != null)
            // one already exists, so we'll just use it.
            return;

        if (m_dispatcherQueueController == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            _ = CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
}
