using BlazorKit.Components;

using Microsoft.Extensions.DependencyInjection;

namespace BlazorKit;

/// <summary>
/// Resolves the native adapter for a Blazor component and lets DI construct it.
/// </summary>
public sealed class NativeAdapterResolver(IServiceProvider services)
    : INativeAdapterResolver
{
    private readonly Dictionary<Type, Type> _registrations = [];

    public void Register<TComponent, TAdapter>()
        where TAdapter : class, INativeAdapter
    {
        _registrations[typeof(TComponent)] = typeof(TAdapter);
    }

    public static NativeAdapterResolver CreateDefault(IServiceProvider services)
    {
        var resolver = new NativeAdapterResolver(services);
        resolver.Register<Label, LabelAdapter>();
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
