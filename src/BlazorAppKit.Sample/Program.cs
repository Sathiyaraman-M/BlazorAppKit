namespace BlazorAppKit.Sample;

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
    private BlazorAppKitHost? _host;

    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

    public override void DidFinishLaunching(NSNotification notification)
    {
        _host = BlazorAppKitHost.Create();

        _window = new NSWindow(
            new CGRect(0, 0, 1024, 700),
            NSWindowStyle.Titled |
            NSWindowStyle.Closable |
            NSWindowStyle.Resizable,
            NSBackingStore.Buffered,
            deferCreation: false)
        {
            Title = "BlazorAppKit Sample",
            ContentViewController = _host.CreateRootViewController<App>(),
            MinSize = new CGSize(760, 520)
        };

        _window.Center();
        _window.MakeKeyAndOrderFront(null);
    }
}

public enum SamplePage
{
    Counter,
    Binding
}

public sealed class SampleState
{
    public event EventHandler? Changed;

    public SamplePage CurrentPage { get; private set; } = SamplePage.Counter;
    public int CurrentCount { get; private set; }
    public string Name { get; private set; } = "Ada";
    public string SearchText { get; private set; } = string.Empty;

    public void SelectPage(SamplePage page)
    {
        if (CurrentPage == page)
        {
            return;
        }

        CurrentPage = page;
        NotifyChanged();
    }

    public void Increment() => SetCount(CurrentCount + 1);

    public void SetName(string value)
    {
        if (Name == value)
        {
            return;
        }

        Name = value;
        NotifyChanged();
    }

    public void SetSearchText(string value)
    {
        if (SearchText == value)
        {
            return;
        }

        SearchText = value;
        NotifyChanged();
    }

    private void SetCount(int value)
    {
        CurrentCount = value;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
