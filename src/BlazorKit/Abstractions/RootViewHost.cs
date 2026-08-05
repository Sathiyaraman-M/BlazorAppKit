namespace BlazorKit.Abstractions;

internal sealed class RootViewHost(NSView view) : IViewContainer
{
    public NSView View { get; } = view;

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
}
