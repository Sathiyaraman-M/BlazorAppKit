using BlazorAppKit.Abstractions;
using BlazorAppKit.Components;

using Microsoft.AspNetCore.Components;

namespace BlazorAppKit.Components.Base;

internal abstract class ViewAdapter<TView>(TView view) : IViewAdapter<TView>
    where TView : AppKit.NSView
{
    private NSViewLayoutOptions _layout = NSViewLayoutOptions.Intrinsic;

    public TView View { get; } = view;

    public NSViewLayoutOptions Layout => _layout;

    protected NativeLayoutState LayoutState { get; } = new();

    AppKit.NSView IViewAdapter.View => View;

    public abstract bool ApplyParameters(ParameterView parameters);

    protected bool ApplyLayout(NSViewLayoutOptions layout)
    {
        if (_layout == layout)
        {
            return false;
        }

        _layout = layout;
        return true;
    }

    public virtual void Dispose()
    {
        LayoutState.Dispose();
        View.RemoveFromSuperview();
        View.Dispose();
    }
}
