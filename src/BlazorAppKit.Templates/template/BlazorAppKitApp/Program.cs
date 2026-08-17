namespace BlazorAppKitApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        NSApplication.Init();
        NSApplication.SharedApplication.Delegate = new ApplicationDelegate();
        NSApplication.Main(args);
    }
}

internal sealed class ApplicationDelegate : NSApplicationDelegate
{
    private NSWindow? _window;
    private BlazorAppKitHost? _host;

    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

    public override void DidFinishLaunching(NSNotification notification)
    {
        _host = BlazorAppKitHost.Create();

        _window = new NSWindow(
            new CGRect(0, 0, 480, 280),
            NSWindowStyle.Titled |
            NSWindowStyle.Closable |
            NSWindowStyle.Resizable,
            NSBackingStore.Buffered,
            deferCreation: false)
        {
            Title = "BlazorAppKitApp",
            ContentViewController = _host.CreateRootViewController<App>()
        };

        _window.Center();
        _window.MakeKeyAndOrderFront(null);
    }
}
