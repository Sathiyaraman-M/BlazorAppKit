using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorKit;

/// <summary>
/// Blazor renderer whose renderer work is serialized onto AppKit's main thread.
/// </summary>
public class AppKitRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, Action<RenderBatch>? displayUpdater = null) : Renderer(serviceProvider, loggerFactory)
{
    private readonly ILogger<AppKitRenderer> _logger = loggerFactory.CreateLogger<AppKitRenderer>();
    private readonly AppKitDispatcher _dispatcher = new();
    private readonly Dictionary<int, NativeNode> _nodes = [];

    public override Dispatcher Dispatcher => _dispatcher;

    /// <summary>
    /// Raised after an exception escapes component rendering or an event callback.
    /// An AppKit host can use this to present an error UI or terminate the app.
    /// </summary>
    public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

    internal INativeAdapterResolver AdapterResolver { get; } = serviceProvider.GetService<INativeAdapterResolver>()
        ?? new NativeAdapterResolver(serviceProvider);

    internal IReadOnlyDictionary<int, NativeNode> Nodes => _nodes;

    protected override void HandleException(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception in the AppKit Blazor renderer.");
        UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(exception, isTerminating: false));
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        // RenderBatch is a ref struct view over renderer-owned memory, so the
        // callback must run before this method returns.
        _dispatcher.AssertAccess();
        displayUpdater?.Invoke(renderBatch);
        return Task.CompletedTask;
    }
}
