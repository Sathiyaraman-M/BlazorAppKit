using BlazorAppKit.Abstractions;

using Microsoft.AspNetCore.Components;

namespace BlazorAppKit.Components.Base;

internal sealed class ViewControllerAdapter : IViewAdapter, IViewContainer, IViewControllerAdapter
{
    private bool _disposed;
    private readonly NativeLayoutState _layoutState = new();

    public ViewControllerAdapter(
        NSViewControllerComponent component,
        BlazorViewController viewController)
    {
        Component = component;
        ViewController = viewController;
        component.AttachViewController(viewController);
    }

    public NSViewControllerComponent Component { get; }

    public NSViewController ViewController { get; }

    public AppKit.NSView View => ViewController.View;

    public NSViewLayoutOptions Layout => Component.Layout;

    AppKit.NSView IViewAdapter.View => View;

    public bool ApplyParameters(ParameterView parameters)
    {
        // Blazor applies parameters to the controller component itself. Unlike
        // generated native-view components, the controller adapter has no
        // separate native property bag to synchronize.
        return false;
    }

    public void SetChildren(IReadOnlyList<IViewAdapter> children)
    {
        NativeLayoutEngine.UpdateRoot(View, _layoutState, children);
    }

    public void SetParentViewController(NSViewController? parent)
    {
        if (ReferenceEquals(ViewController.ParentViewController, parent))
        {
            return;
        }

        ViewController.RemoveFromParentViewController();
        parent?.AddChildViewController(ViewController);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ViewController.RemoveFromParentViewController();
        View.RemoveFromSuperview();
        _layoutState.Dispose();
        ViewController.Dispose();
    }
}
