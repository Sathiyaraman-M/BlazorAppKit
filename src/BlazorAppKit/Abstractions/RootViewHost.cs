namespace BlazorAppKit.Abstractions;

internal sealed class RootViewHost(global::AppKit.NSView view) : IViewContainer
{
    private readonly Components.Base.NativeLayoutState _layoutState = new();

    public global::AppKit.NSView View { get; } = view;

    public void SetChildren(IReadOnlyList<IViewAdapter> children)
    {
        Components.Base.NativeLayoutEngine.UpdateRoot(View, _layoutState, children);
    }
}
