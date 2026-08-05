using BlazorKit.Abstractions;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components.Base;

internal abstract class NativeAdapter<TView>(TView view) : INativeAdapter<TView>
    where TView : NSView
{
    public TView View { get; } = view;

    NSView INativeAdapter.View => View;

    public abstract void ApplyParameters(ParameterView parameters);

    public virtual void Dispose()
    {
        View.RemoveFromSuperview();
        View.Dispose();
    }
}
