using BlazorAppKit.Abstractions;

using Microsoft.AspNetCore.Components;

namespace BlazorAppKit.Components.Base;

internal sealed class ViewControllerAdapter : IViewAdapter, IViewContainer, IViewControllerAdapter
{
    private bool _disposed;

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

    AppKit.NSView IViewAdapter.View => View;

    public void ApplyParameters(ParameterView parameters)
    {
        // Blazor applies parameters to the controller component itself. Unlike
        // generated native-view components, the controller adapter has no
        // separate native property bag to synchronize.
    }

    public void SetChildren(IReadOnlyList<IViewAdapter> children)
    {
        foreach (var child in View.Subviews)
        {
            child.RemoveFromSuperview();
        }

        foreach (var child in children)
        {
            child.View.Frame = View.Bounds;
            child.View.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
            View.AddSubview(child.View);
        }
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
        ViewController.Dispose();
    }
}
