#nullable enable
using DCompPrivateProjection.ABI;
using Microsoft.UI.Composition.Private;
using Microsoft.UI.Xaml.Hosting;
using System;
using Windows.UI.Composition;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace WebView2Ex.UI;

partial class WebView2Ex
{
    static Guid IIDPlatformIVisual = new Guid("117E202D-A859-4C89-873B-C2AA566788E3");
    IDisposable[]? Disposables;

    [StructLayout(LayoutKind.Sequential)]
    struct DispatcherQueueOptions
    {
        internal int dwSize;
        internal int threadType;
        internal int apartmentType;
    }

    [DllImport("CoreMessaging.dll")]
    private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

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
            var MUXPartner = MUXCompositor.GetPartnerInstance();
            var MUXProxyInstance = SystemVisualProxyVisualPrivate.Create(MUXCompositor);
            var MUXProxyVisual = MUXProxyInstance.AsVisual();
            EnsureWindowsSystemDispatcherQueueController();
            var WUXCompositor = new Compositor();
            var WUXPartner = WUXCompositor.GetPartnerInstance();
            var TargetFromUndockedComposition = WUXPartner.OpenShardTargetFromHandle(MUXProxyInstance.GetHandle());

            var WUXVisual = WUXCompositor.CreateContainerVisual();

            TargetFromUndockedComposition.SetRoot(WUXVisual);

            await WUXCompositor.RequestCommitAsync();
            await MUXCompositor.RequestCommitAsync();

            Disposables = Array<IDisposable>(
                MUXPartner, MUXProxyInstance, MUXProxyVisual,
                WUXCompositor, WUXPartner, TargetFromUndockedComposition);

            // Use this Visual with WebView2
            return (MUXProxyVisual, WUXVisual);
        } catch
        {
            Debugger.Break();
            return (null!, null!);
        }
    }
    static T[] Array<T>(params T[] array) => array;
}
