# WebView2Ex

Reimplementation of WebView2 control in both UWP and WinUI 3

Most of the code is translated from the microsoft-ui-xaml repository. Some changes are made to support more features or fix bugs.

## Differences with the built-in WebView2 control
- You get full access to the CoreWebView2CompositionController class.
- I have separated the runtime from the control. This means that the runtime can be viewed as a "data class" or "Model" and WebView2Ex control can be viewed as a "control class" or "View"
- (UWP) Partially supports AppWindow
- I have fixed horizontal scrolling (not sure if the built-in control fixed that already or not, but at the time it was an issue)
- Smooth touchpad scrolling (not as natively but it at least feels better than the built-in version)

## UWP notes
- To use WebView2 in AppWindow, call WebView2Ex.SetWindow(AppWindow) for support with AppWindow.
- The AppWindow version is usable but I am not sure if there are any other bugs currently.

## WinUI 3 notes

I have implemented two modes. It can use both the WebView2 that is shipped with WASDK itself and the external WebView2 Nuget package. You can change them in the csproj.

The external Nuget package version
- Can upgrade to a newer version before Microsoft releases the new version of WASDK
- Can try experimental or preview WebView2 features on WinUI 3
- Limitation: Changing cursor shape is not yet implemented. The cursor will stay the default cursor all the time.
- Limitation: You need to alias the WebView2 Nuget package. So, it will look something like `extern alias WV2; using WV2::Microsoft.Web.WebView2;` instead.

The WebView2 that is shipped with WASDK
- Changing cursor shape is implemented. Does not have the same limitation.
- However, the new feature of WebView2 will have to wait for the new release of WASDK

To change to use WebView2 which is already shipped with WASDK
- In both WebView2Ex.WinUI and WebView2ExTest.WinUI, remove the defined constant `NonWinRTWebView2`.
- Remove the `PackageReference` that installs WebView2
