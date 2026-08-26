using Microsoft.AspNetCore.Components;

namespace BlazorAppKit.Abstractions;

internal interface IViewAdapter : IDisposable
{
    NSView View { get; }

    Components.NSViewLayoutOptions Layout { get; }

    bool ApplyParameters(ParameterView parameters);
}

internal interface IViewAdapter<out TView> : IViewAdapter
    where TView : NSView
{
    new TView View { get; }
}
