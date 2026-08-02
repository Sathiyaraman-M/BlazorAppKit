namespace BlazorKit;

/// <summary>
/// Non-generic adapter contract used by the renderer to store heterogeneous
/// native peers in one tree.
/// </summary>
public interface INativeAdapter : IDisposable
{
    NSObject NativeObject { get; }

    void SetParameter(string name, object? value);
}

/// <summary>
/// Adapter contract for AppKit view-backed components.
/// </summary>
public interface INativeViewAdapter : INativeAdapter
{
    NSView View { get; }
}

public interface INativeAdapter<out TView> : INativeViewAdapter
    where TView : NSView
{
    new TView View { get; }
}

/// <summary>
/// Adapter contract for controls that own child views.
/// </summary>
public interface INativeViewContainerAdapter : INativeAdapter
{
    void AddChild(INativeViewAdapter child);

    void RemoveChild(INativeViewAdapter child);
}

/// <summary>
/// Adapter contract for native windows. NSWindow is not an NSView.
/// </summary>
public interface INativeWindowAdapter : INativeAdapter
{
    NSWindow Window { get; }
}
