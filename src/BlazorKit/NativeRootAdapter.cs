namespace BlazorKit;

internal sealed class NativeRootAdapter(NSView view) : INativeViewContainerAdapter
{
    public NSView View { get; } = view;

    NSObject INativeAdapter.NativeObject => View;

    public void SetParameter(string name, object? value)
    {
        // The root adapter represents the host-provided NSView. Root component
        // parameters belong to the component and are not native view properties.
    }

    public void AddChild(INativeViewAdapter child)
    {
        child.View.Frame = View.Bounds;
        child.View.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
        View.AddSubview(child.View);
    }

    public void RemoveChild(INativeViewAdapter child)
    {
        child.View.RemoveFromSuperview();
    }

    public void Dispose()
    {
        // The host owns the root NSView.
    }
}
