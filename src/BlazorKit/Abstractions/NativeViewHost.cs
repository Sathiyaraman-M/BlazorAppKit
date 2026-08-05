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
            if (child.NativeObject is not NSView nativeView)
                throw new NotSupportedException("The native host can only contain NSView instances.");
            nativeView.Frame = View.Bounds;
            nativeView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
            View.AddSubview(nativeView);
        }
    }
}
