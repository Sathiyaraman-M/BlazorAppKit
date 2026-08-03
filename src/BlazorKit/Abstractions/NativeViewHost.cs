namespace BlazorKit.Abstractions;

internal sealed class NativeViewHost(NSView view) : INativeViewContainer
{
    public NSView View { get; } = view;

    public void SetChildren(IReadOnlyList<INativeViewAdapter> children)
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
}
