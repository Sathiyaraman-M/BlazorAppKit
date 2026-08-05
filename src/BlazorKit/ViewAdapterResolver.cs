using BlazorKit.Abstractions;
using BlazorKit.Components;

using Microsoft.Extensions.DependencyInjection;

namespace BlazorKit;

/// <summary>
/// Resolves the native adapter for a Blazor component and lets DI construct it.
/// </summary>
internal sealed class ViewAdapterResolver(IServiceProvider services)
{
    private readonly Dictionary<Type, Type> _registrations = [];

    public void Register<TComponent, TAdapter>()
        where TAdapter : class, IViewAdapter
    {
        _registrations[typeof(TComponent)] = typeof(TAdapter);
    }

    public static ViewAdapterResolver CreateDefault(IServiceProvider services)
    {
        var resolver = new ViewAdapterResolver(services);
        resolver.Register<Components.NSTextField, NSTextFieldAdapter>();
        resolver.Register<Components.NSButton, NSButtonAdapter>();
        resolver.Register<Components.NSStackView, NSStackViewAdapter>();
        resolver.Register<Components.NSSlider, NSSliderAdapter>();
        resolver.Register<Components.NSProgressIndicator, NSProgressIndicatorAdapter>();
        resolver.Register<Components.NSScrollView, NSScrollViewAdapter>();
        return resolver;
    }

    public IViewAdapter Create(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        if (!_registrations.TryGetValue(componentType, out var adapterType))
        {
            throw new NotSupportedException(
                $"No native AppKit adapter is registered for component '{componentType.FullName}'.");
        }

        return (IViewAdapter)ActivatorUtilities.CreateInstance(services, adapterType);
    }
}
