using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using System.Diagnostics.CodeAnalysis;

namespace BlazorAppKit;

/// <summary>
/// Owns the BlazorAppKit services and renderer for an AppKit application.
/// AppKit application and window lifecycle remains the responsibility of the caller.
/// </summary>
public sealed class BlazorAppKitHost : IDisposable
{
    private bool _disposed;

    private BlazorAppKitHost(IServiceProvider serviceProvider, AppKitRenderer renderer)
    {
        Services = serviceProvider;
        Renderer = renderer;
    }

    /// <summary>
    /// Creates a host and invokes <paramref name="configureServices"/> to register application services.
    /// </summary>
    public static BlazorAppKitHost Create(Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<AppKitRenderer>();
        configureServices?.Invoke(services);

        var serviceProvider = services.BuildServiceProvider();
        return new BlazorAppKitHost(
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

    /// <summary>
    /// Creates a native view controller that hosts a root Blazor view-controller component.
    /// </summary>
    /// <typeparam name="TComponent">The root view-controller component type.</typeparam>
    /// <returns>A native controller suitable for <c>NSWindow.ContentViewController</c>.</returns>
    public NSViewController CreateRootViewController<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>()
        where TComponent : Components.NSViewControllerComponent =>
        Renderer.CreateRootViewController<TComponent>();

    /// <summary>
    /// Creates a native view controller that hosts a parameterized root Blazor view-controller component.
    /// </summary>
    /// <typeparam name="TComponent">The root view-controller component type.</typeparam>
    /// <param name="parameters">The parameters supplied to the root component.</param>
    /// <returns>A native controller suitable for <c>NSWindow.ContentViewController</c>.</returns>
    public NSViewController CreateRootViewController<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters)
        where TComponent : Components.NSViewControllerComponent =>
        Renderer.CreateRootViewController<TComponent>(parameters);

    /// <summary>
    /// Releases the renderer and service provider owned by this host.
    /// </summary>
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
