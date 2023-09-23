#nullable enable
using Microsoft.UI.Xaml.Hosting;
using System;
using Windows.UI.Composition;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinRT;

namespace WebView2Ex.UI;

partial class WebView2Ex
{
    IDisposable[]? Disposables;

    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

    object m_dispatcherQueueController = null;
    void EnsureWindowsSystemDispatcherQueueController()
    {
        if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
        {
            // one already exists, so we'll just use it.
            return;
        }

        if (m_dispatcherQueueController == null)
        {
            DispatcherQueueOptions options;
            options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
            options.threadType = 2;    // DQTYPE_THREAD_CURRENT
            options.apartmentType = 2; // DQTAT_COM_STA

            CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
        }
    }
    async Task<(Microsoft.UI.Composition.Visual MUXVisual, Visual WUXVisual)> CreateVisual()
    {
        try
        {
            var MUXCompositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            var CEOL = Microsoft.UI.Content.ContentExternalOutputLink.Create(MUXCompositor);
            var WUXTarget = CompositionTarget.FromAbi(
                ((IWinRTObject)CEOL).NativeObject.GetRef()
            );
            
            var MUXProxyVisual = CEOL.PlacementVisual;
            EnsureWindowsSystemDispatcherQueueController();

            var WUXCompositor = new Compositor();
            var WUXVisual = WUXCompositor.CreateContainerVisual();
            WUXTarget.Root = WUXVisual;

            await MUXCompositor.RequestCommitAsync();
            await WUXCompositor.RequestCommitAsync();

            Disposables = Array<IDisposable>(
                CEOL, MUXProxyVisual
            );

            // Use this Visual with WebView2
            return (MUXProxyVisual, WUXVisual);
            //return (null!, null!);
        } catch
        {
            Debugger.Break();
            return (null!, null!);
        }
    }
    static T[] Array<T>(params T[] array) => array;
}