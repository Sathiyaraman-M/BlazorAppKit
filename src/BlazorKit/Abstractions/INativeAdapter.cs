using Microsoft.AspNetCore.Components;

namespace BlazorKit.Abstractions;

internal interface INativeAdapter : IDisposable
{
    NSView View { get; }

    void ApplyParameters(ParameterView parameters);
}

internal interface INativeAdapter<out TView> : INativeAdapter
    where TView : NSView
{
    new TView View { get; }
}
