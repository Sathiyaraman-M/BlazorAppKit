namespace BlazorKit;

internal sealed class NativeRootAdapter(NSView view) : INativeContainerAdapter
{
    public NSView View { get; } = view;

    NSObject INativeAdapter.NativeObject => View;

    public void SetParameter(string name, object? value)
    {
        // The root adapter represents the host-provided NSView. Root component
        // parameters belong to the component and are not native view properties.
    }

    public void InsertChild(INativeAdapter child, int index)
    {
        if (child.NativeObject is not NSView childView)
        {
            throw new InvalidOperationException("The root host accepts only NSView adapters.");
        }

        childView.Frame = View.Bounds;
        childView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
        View.AddSubview(childView);
    }

    public void RemoveChild(INativeAdapter child)
    {
        if (child.NativeObject is NSView childView)
        {
            childView.RemoveFromSuperview();
        }
    }

    public void Dispose()
    {
        // The host owns the root NSView.
    }
}
