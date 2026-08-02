namespace BlazorKit.Components.Base;

internal abstract class NativeViewAdapter<TView>(TView view) : INativeAdapter<TView>
    where TView : NSView
{
    public TView View { get; } = view;

    NSView INativeViewAdapter.View => View;
    NSObject INativeAdapter.NativeObject => View;

    public abstract void SetParameter(string name, object? value);

    public virtual void Dispose()
    {
        View.RemoveFromSuperview();
        View.Dispose();
    }
}
