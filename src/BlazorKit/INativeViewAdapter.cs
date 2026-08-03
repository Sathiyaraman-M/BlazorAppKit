namespace BlazorKit;

public interface INativeViewAdapter : IDisposable
{
    NSView View { get; }

    void SetParameter(string name, object? value);
}

public interface INativeViewAdapter<out TView> : INativeViewAdapter
    where TView : NSView
{
    new TView View { get; }
}
