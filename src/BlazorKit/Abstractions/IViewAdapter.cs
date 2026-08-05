using Microsoft.AspNetCore.Components;

namespace BlazorKit.Abstractions;

internal interface IViewAdapter : IDisposable
{
    NSView View { get; }

    void ApplyParameters(ParameterView parameters);
}

internal interface IViewAdapter<out TView> : IViewAdapter
    where TView : NSView
{
    new TView View { get; }
}
