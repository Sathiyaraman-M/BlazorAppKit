namespace BlazorKit.Abstractions;

using Microsoft.AspNetCore.Components;

internal interface INativeAdapter : IDisposable
{
    NSObject NativeObject { get; }

    void ApplyParameters(ParameterView parameters);
}

internal interface INativeAdapter<out TNative> : INativeAdapter
    where TNative : NSObject
{
    new TNative NativeObject { get; }
}

// Compatibility aliases for the original renderer vocabulary.
internal interface INativeViewAdapter : INativeAdapter
{
    NSView View { get; }
}

internal interface INativeViewAdapter<out TView> : INativeViewAdapter
    where TView : NSView
{
    new TView View { get; }
}
