using BlazorKit.Abstractions;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components.Base;

internal abstract class NativeViewAdapter<TView>(TView view) : INativeViewAdapter<TView>
    where TView : NSView
{
    public TView View { get; } = view;

    public NSObject NativeObject => View;

    NSView INativeViewAdapter.View => View;
    NSObject INativeAdapter.NativeObject => View;

    public abstract void ApplyParameters(ParameterView parameters);

    public virtual void Dispose()
    {
        View.RemoveFromSuperview();
        View.Dispose();
    }
}

internal abstract class NativeAdapter<TNative>(TNative nativeObject) : INativeAdapter<TNative>
    where TNative : NSObject
{
    public TNative NativeObject { get; } = nativeObject;

    NSObject INativeAdapter.NativeObject => NativeObject;

    public abstract void ApplyParameters(ParameterView parameters);

    public virtual void Dispose()
    {
        if (NativeObject is NSView view) view.RemoveFromSuperview();
        NativeObject.Dispose();
    }
}
