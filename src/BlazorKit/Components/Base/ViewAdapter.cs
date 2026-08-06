using BlazorKit.Abstractions;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components.Base;

internal abstract class ViewAdapter<TView>(TView view) : IViewAdapter<TView>
    where TView : AppKit.NSView
{
    public TView View { get; } = view;

    AppKit.NSView IViewAdapter.View => View;

    public abstract void ApplyParameters(ParameterView parameters);

    public virtual void Dispose()
    {
        View.RemoveFromSuperview();
        View.Dispose();
    }
}
