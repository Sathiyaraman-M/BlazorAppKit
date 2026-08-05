namespace BlazorKit.Abstractions;

internal sealed class NativeViewHost(NSView view) : INativeContainer
{
    public NSView View { get; } = view;

    public void SetChildren(IReadOnlyList<INativeAdapter> children)
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
