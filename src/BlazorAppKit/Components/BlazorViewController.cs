namespace BlazorAppKit.Components;

/// <summary>
/// Native controller peer used by a <see cref="NSViewControllerComponent"/>.
/// </summary>
internal sealed class BlazorViewController : NSViewController
{
    private readonly Func<BlazorViewController, Task>? _viewDidLoadHandler;
    private NSViewControllerComponent? _component;
    private bool _viewLoaded;
    private bool _viewDidLoad;

    public BlazorViewController(NSViewControllerComponent component)
    {
        _component = component;
    }

    public BlazorViewController(Func<BlazorViewController, Task> viewDidLoadHandler)
    {
        _viewDidLoadHandler = viewDidLoadHandler;
    }

    public void AttachComponent(NSViewControllerComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        _component = component;

        if (_viewLoaded)
        {
            component.NotifyLoadView();
        }

        if (_viewDidLoad)
        {
            component.NotifyViewDidLoad();
        }
    }

    public override void LoadView()
    {
        View = new AppKit.NSView();
        _viewLoaded = true;
        _component?.NotifyLoadView();
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        _viewDidLoad = true;
        _component?.NotifyViewDidLoad();

        if (_viewDidLoadHandler is not null)
        {
            _ = _viewDidLoadHandler(this);
        }
    }

    public override void ViewWillAppear()
    {
        base.ViewWillAppear();
        _component?.NotifyViewWillAppear();
    }

    public override void ViewDidAppear()
    {
        base.ViewDidAppear();
        _component?.NotifyViewDidAppear();
    }

    public override void ViewWillDisappear()
    {
        base.ViewWillDisappear();
        _component?.NotifyViewWillDisappear();
    }

    public override void ViewDidDisappear()
    {
        base.ViewDidDisappear();
        _component?.NotifyViewDidDisappear();
    }
}
