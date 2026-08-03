using BlazorKit.Abstractions;

namespace BlazorKit.Components.Base;

internal abstract class NativeViewAdapter<TView>(TView view) : INativeViewAdapter<TView>
    where TView : NSView
{
    public TView View { get; } = view;

    NSView INativeViewAdapter.View => View;

    public abstract void SetParameter(string name, object? value);

    public virtual void Dispose()
    {
        View.RemoveFromSuperview();
        View.Dispose();
    }
}
