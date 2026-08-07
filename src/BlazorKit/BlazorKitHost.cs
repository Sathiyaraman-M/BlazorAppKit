using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.CodeAnalysis;

namespace BlazorKit;

/// <summary>
/// Owns the BlazorKit services and renderer for an AppKit application.
/// AppKit application and window lifecycle remains the responsibility of the caller.
/// </summary>
public sealed class BlazorKitHost : IDisposable
{
    private bool _disposed;

    private BlazorKitHost(IServiceProvider serviceProvider, AppKitRenderer renderer)
    {
        Services = serviceProvider;
        Renderer = renderer;
    }

    /// <summary>
    /// Creates a host and invokes <paramref name="configureServices"/> to register application services.
    /// </summary>
    public static BlazorKitHost Create(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<AppKitRenderer>();
        configureServices?.Invoke(services);

        var serviceProvider = services.BuildServiceProvider();
        return new BlazorKitHost(
            serviceProvider,
            serviceProvider.GetRequiredService<AppKitRenderer>());
    }

    /// <summary>
    /// The service provider owned by this host.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// The renderer owned by this host.
    /// </summary>
    public AppKitRenderer Renderer { get; }

    /// <summary>
    /// Mounts a Blazor root component into an AppKit view.
    /// </summary>
    public Task<int> MountRootComponentAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        NSView host)
        where TComponent : IComponent =>
        Renderer.MountRootComponentAsync<TComponent>(host);

    /// <summary>
    /// Mounts a Blazor root component with parameters into an AppKit view.
    /// </summary>
    public Task<int> MountRootComponentAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        NSView host,
        ParameterView parameters)
        where TComponent : IComponent =>
        Renderer.MountRootComponentAsync<TComponent>(host, parameters);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Renderer.Dispose();
        if (Services is IDisposable disposableServices)
        {
            disposableServices.Dispose();
        }
    }
}
