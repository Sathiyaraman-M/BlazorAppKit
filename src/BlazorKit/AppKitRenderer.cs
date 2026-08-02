using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;

namespace BlazorKit;

/// <summary>
/// Blazor renderer whose renderer work is serialized onto AppKit's main thread.
/// </summary>
public class AppKitRenderer(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory,
    Action<RenderBatch>? displayUpdater = null) : Renderer(serviceProvider, loggerFactory)
{
    private readonly ILogger<AppKitRenderer> _logger = loggerFactory.CreateLogger<AppKitRenderer>();
    private readonly AppKitDispatcher _dispatcher = new();
    private readonly Dictionary<int, NativeNode> _nodes = [];

    public override Dispatcher Dispatcher => _dispatcher;

    public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

    internal INativeAdapterResolver AdapterResolver { get; } =
        serviceProvider.GetService<INativeAdapterResolver>()
        ?? NativeAdapterResolver.CreateDefault(serviceProvider);

    internal IReadOnlyDictionary<int, NativeNode> Nodes => _nodes;

    /// <summary>
    /// Mounts a native-aware Blazor root component into an AppKit view.
    /// </summary>
    public Task<int> MountRootComponentAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        NSView host,
        ParameterView parameters = default)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(host);

        return Dispatcher.InvokeAsync(async () =>
        {
            var component = InstantiateComponent(typeof(TComponent));
            var componentId = AssignRootComponentId(component);
            var adapter = new NativeRootAdapter(host);
            var node = new NativeNode(componentId, adapter);

            _nodes.Add(componentId, node);

            await RenderRootComponentAsync(componentId, parameters);
            return componentId;
        });
    }

    protected override void HandleException(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception in the AppKit Blazor renderer.");
        UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(exception, false));
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        _dispatcher.AssertAccess();

        for (var i = 0; i < renderBatch.UpdatedComponents.Count; i++)
        {
            ApplyComponentRenderTree(renderBatch.UpdatedComponents.Array[i].ComponentId);
        }

        for (var i = 0; i < renderBatch.DisposedComponentIDs.Count; i++)
        {
            DisposeNode(renderBatch.DisposedComponentIDs.Array[i]);
        }

        // RenderBatch is a view over renderer-owned memory, so this callback
        // must run before UpdateDisplayAsync returns.
        displayUpdater?.Invoke(renderBatch);
        return Task.CompletedTask;
    }

    private void ApplyComponentRenderTree(int componentId)
    {
        if (!_nodes.TryGetValue(componentId, out var parent) ||
            parent.Adapter is not INativeContainerAdapter container)
        {
            return;
        }

        var frames = GetCurrentRenderTreeFrames(componentId);
        var seen = new HashSet<int>();
        var siblingIndex = 0;

        foreach (var frameIndex in EnumerateComponentFrames(frames.Array, 0, frames.Count))
        {
            var frame = frames.Array[frameIndex];
            var child = EnsureNode(parent, frame, frames.Array, frameIndex);
            seen.Add(child.ComponentId);

            if (child.Parent is null)
            {
                child.Parent = parent;
                parent.Children.Insert(siblingIndex, child);
                container.InsertChild(child.Adapter, siblingIndex);
            }

            siblingIndex++;
        }

        for (var i = parent.Children.Count - 1; i >= 0; i--)
        {
            var child = parent.Children[i];
            if (!seen.Contains(child.ComponentId))
            {
                parent.Children.RemoveAt(i);
                container.RemoveChild(child.Adapter);
                DisposeSubtree(child);
            }
        }
    }

    private NativeNode EnsureNode(
        NativeNode parent,
        RenderTreeFrame frame,
        RenderTreeFrame[] frames,
        int frameIndex)
    {
        if (_nodes.TryGetValue(frame.ComponentId, out var existing))
        {
            ApplyParameters(existing.Adapter, frames, frameIndex + 1, frame.ComponentSubtreeLength - 1);
            return existing;
        }

        var adapter = AdapterResolver.Create(frame.ComponentType);
        var node = new NativeNode(frame.ComponentId, adapter);
        _nodes.Add(node.ComponentId, node);
        ApplyParameters(adapter, frames, frameIndex + 1, frame.ComponentSubtreeLength - 1);
        return node;
    }

    private static IEnumerable<int> EnumerateComponentFrames(
        RenderTreeFrame[] frames,
        int start,
        int count)
    {
        var end = Math.Min(start + count, frames.Length);
        for (var index = start; index < end; index++)
        {
            var frame = frames[index];
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Component:
                    yield return index;
                    index += frame.ComponentSubtreeLength - 1;
                    break;
                case RenderTreeFrameType.Region:
                    foreach (var nested in EnumerateComponentFrames(
                        frames,
                        index + 1,
                        frame.RegionSubtreeLength - 1))
                    {
                        yield return nested;
                    }

                    index += frame.RegionSubtreeLength - 1;
                    break;
                case RenderTreeFrameType.Element:
                    throw new NotSupportedException(
                        $"HTML element '{frame.ElementName}' is not supported by the AppKit renderer.");
            }
        }
    }

    private static void ApplyParameters(
        INativeAdapter adapter,
        RenderTreeFrame[] frames,
        int start,
        int count)
    {
        var end = Math.Min(start + count, frames.Length);
        for (var index = start; index < end; index++)
        {
            var frame = frames[index];
            if (frame.FrameType == RenderTreeFrameType.Attribute)
            {
                adapter.SetParameter(frame.AttributeName, frame.AttributeValue);
            }
        }
    }

    private void DisposeNode(int componentId)
    {
        if (_nodes.TryGetValue(componentId, out var node))
        {
            node.Parent?.Children.Remove(node);
            DisposeSubtree(node);
        }
    }

    private void DisposeSubtree(NativeNode node)
    {
        foreach (var child in node.Children.ToArray())
        {
            DisposeSubtree(child);
        }

        node.Children.Clear();
        _nodes.Remove(node.ComponentId);
        node.Adapter.Dispose();
    }
}
