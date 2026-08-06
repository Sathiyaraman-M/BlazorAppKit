using BlazorKit.Abstractions;
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
        GeneratedViewAdapterRegistrations.Register(resolver);
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
