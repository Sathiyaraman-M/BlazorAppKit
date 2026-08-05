using BlazorKit.Abstractions;
using BlazorKit.Components;

using Microsoft.Extensions.DependencyInjection;

namespace BlazorKit;

/// <summary>
/// Resolves the native adapter for a Blazor component and lets DI construct it.
/// </summary>
internal sealed class NativeViewAdapterResolver(IServiceProvider services)
{
    private readonly Dictionary<Type, Type> _registrations = [];

    public void Register<TComponent, TAdapter>()
        where TAdapter : class, INativeAdapter
    {
        _registrations[typeof(TComponent)] = typeof(TAdapter);
    }

    public static NativeViewAdapterResolver CreateDefault(IServiceProvider services)
    {
        var resolver = new NativeViewAdapterResolver(services);
        resolver.Register<BlazorKit.Components.NSTextField, NSTextFieldAdapter>();
        resolver.Register<BlazorKit.Components.NSButton, NSButtonAdapter>();
        resolver.Register<BlazorKit.Components.NSStackView, NSStackViewAdapter>();
        resolver.Register<BlazorKit.Components.NSSlider, NSSliderAdapter>();
        resolver.Register<BlazorKit.Components.NSProgressIndicator, NSProgressIndicatorAdapter>();
        resolver.Register<BlazorKit.Components.NSScrollView, NSScrollViewAdapter>();
        resolver.Register<Label, LabelAdapter>();
        resolver.Register<TextField, TextFieldAdapter>();
        resolver.Register<Button, ButtonAdapter>();
        resolver.Register<VStack, VStackAdapter>();
        resolver.Register<HStack, HStackAdapter>();
        return resolver;
    }

    public INativeAdapter Create(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        if (!_registrations.TryGetValue(componentType, out var adapterType))
        {
            throw new NotSupportedException(
                $"No native AppKit adapter is registered for component '{componentType.FullName}'.");
        }

        return (INativeAdapter)ActivatorUtilities.CreateInstance(services, adapterType);
    }
}
