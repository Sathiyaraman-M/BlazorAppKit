namespace BlazorKit.Sample;

internal static class Program
{
    private static void Main(string[] args)
    {
        NSApplication.Init();
        NSApplication.SharedApplication.Delegate = new SampleApplicationDelegate();
        NSApplication.Main(args);
    }
}

internal sealed class SampleApplicationDelegate : NSApplicationDelegate
{
    private NSWindow? _window;
    private BlazorKitHost? _host;

    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

    public override void DidFinishLaunching(NSNotification notification)
    {
        _window = new NSWindow(
            new CGRect(0, 0, 480, 280),
            NSWindowStyle.Titled |
            NSWindowStyle.Closable |
            NSWindowStyle.Resizable,
            NSBackingStore.Buffered,
            deferCreation: false)
        {
            Title = "BlazorKit Counter"
        };

        _host = BlazorKitHost.Create();

        if (_window.ContentView is { } contentView)
        {
            _ = _host.MountRootComponentAsync<App>(contentView);
        }

        _window.Center();
        _window.MakeKeyAndOrderFront(null);
    }
}
