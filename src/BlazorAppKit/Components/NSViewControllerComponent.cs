using Microsoft.AspNetCore.Components;

namespace BlazorAppKit.Components;

/// <summary>
/// Blazor component base for an AppKit view controller.
/// </summary>
/// <remarks>
/// The component owns a native view-controller peer. Its rendered child
/// components become the contents of that controller's view, while nested
/// controller components participate in native controller containment.
/// </remarks>
public abstract class NSViewControllerComponent : ComponentBase, IDisposable
{
    private BlazorViewController? _viewController;
    private bool _disposed;

    /// <summary>
    /// Gets the native view controller owned by this component.
    /// </summary>
    public NSViewController ViewController => _viewController
        ?? throw new InvalidOperationException("The view controller is not attached to a renderer.");

    /// <summary>
    /// Gets the native view owned by <see cref="ViewController"/>.
    /// </summary>
    public AppKit.NSView View => ViewController.View;

    /// <summary>
    /// Gets the native parent view controller, if one exists.
    /// </summary>
    public NSViewController? ParentViewController => ViewController.ParentViewController;

    /// <summary>
    /// Gets the native child view controllers.
    /// </summary>
    public IReadOnlyList<NSViewController> ChildViewControllers => ViewController.ChildViewControllers;

    /// <summary>
    /// Gets whether the native view has been loaded.
    /// </summary>
    public bool IsViewLoaded => ViewController.ViewLoaded;

    /// <summary>
    /// Gets the native view if it has already been loaded; otherwise returns <see langword="null"/>.
    /// </summary>
    public AppKit.NSView? ViewIfLoaded => ViewController.ViewIfLoaded;

    /// <summary>
    /// Ensures that the native view has been loaded.
    /// </summary>
    public void LoadViewIfNeeded() => ViewController.LoadViewIfNeeded();

    /// <summary>
    /// Adds a native child view controller to this controller.
    /// </summary>
    public void AddChildViewController(NSViewController childViewController)
    {
        ArgumentNullException.ThrowIfNull(childViewController);
        ViewController.AddChildViewController(childViewController);
    }

    /// <summary>
    /// Removes this controller from its native parent view controller.
    /// </summary>
    public void RemoveFromParentViewController() => ViewController.RemoveFromParentViewController();

    /// <summary>
    /// Presents a native view controller from this controller.
    /// </summary>
    public void PresentViewController(NSViewController viewController, INSViewControllerPresentationAnimator animator)
    {
        ArgumentNullException.ThrowIfNull(viewController);
        ArgumentNullException.ThrowIfNull(animator);
        ViewController.PresentViewController(viewController, animator);
    }

    /// <summary>
    /// Presents a native view controller as a modal window.
    /// </summary>
    public void PresentViewControllerAsModalWindow(NSViewController viewController)
    {
        ArgumentNullException.ThrowIfNull(viewController);
        ViewController.PresentViewControllerAsModalWindow(viewController);
    }

    /// <summary>
    /// Presents a native view controller as a sheet.
    /// </summary>
    public void PresentViewControllerAsSheet(NSViewController viewController)
    {
        ArgumentNullException.ThrowIfNull(viewController);
        ViewController.PresentViewControllerAsSheet(viewController);
    }

    /// <summary>
    /// Gets the native controller presenting this controller, if one exists.
    /// </summary>
    public NSViewController? PresentingViewController => ViewController.PresentingViewController;

    /// <summary>
    /// Gets the native controllers presented by this controller.
    /// </summary>
    public IReadOnlyList<NSViewController> PresentedViewControllers => ViewController.PresentedViewControllers;

    /// <summary>
    /// Dismisses the native view controller presented by this controller.
    /// </summary>
    public void DismissViewController(NSViewController viewController)
    {
        ArgumentNullException.ThrowIfNull(viewController);
        ViewController.DismissViewController(viewController);
    }

    /// <summary>
    /// Called when the native view is being loaded.
    /// </summary>
    protected virtual void LoadView()
    {
    }

    /// <summary>
    /// Called after the native view has loaded.
    /// </summary>
    protected virtual void ViewDidLoad()
    {
    }

    /// <summary>
    /// Called immediately before the controller's view is added to a visible hierarchy.
    /// </summary>
    protected virtual void ViewWillAppear()
    {
    }

    /// <summary>
    /// Called after the controller's view is added to a visible hierarchy.
    /// </summary>
    protected virtual void ViewDidAppear()
    {
    }

    /// <summary>
    /// Called immediately before the controller's view is removed from a visible hierarchy.
    /// </summary>
    protected virtual void ViewWillDisappear()
    {
    }

    /// <summary>
    /// Called after the controller's view is removed from a visible hierarchy.
    /// </summary>
    protected virtual void ViewDidDisappear()
    {
    }

    /// <summary>
    /// Called when the component is disposed.
    /// </summary>
    protected virtual void OnDispose()
    {
    }

    internal void AttachViewController(BlazorViewController viewController)
    {
        ArgumentNullException.ThrowIfNull(viewController);

        if (_viewController is not null && !ReferenceEquals(_viewController, viewController))
        {
            throw new InvalidOperationException("A view controller component cannot be attached more than once.");
        }

        _viewController = viewController;
        viewController.AttachComponent(this);
    }

    internal void NotifyLoadView() => LoadView();

    internal void NotifyViewDidLoad() => ViewDidLoad();

    internal void NotifyViewWillAppear() => ViewWillAppear();

    internal void NotifyViewDidAppear() => ViewDidAppear();

    internal void NotifyViewWillDisappear() => ViewWillDisappear();

    internal void NotifyViewDidDisappear() => ViewDidDisappear();

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        OnDispose();
    }
}
