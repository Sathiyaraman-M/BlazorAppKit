using BlazorAppKit.Abstractions;
using BlazorAppKit.Components.Base;

using BlazorViewController = BlazorAppKit.Components.BlazorViewController;
using NSViewControllerComponent = BlazorAppKit.Components.NSViewControllerComponent;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;

namespace BlazorAppKit;

/// <summary>
/// Blazor renderer whose renderer work is serialized onto AppKit's main thread.
/// </summary>
public class AppKitRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : Renderer(serviceProvider, loggerFactory)
{
    private readonly ILogger<AppKitRenderer> _logger = loggerFactory.CreateLogger<AppKitRenderer>();
    private readonly AppKitDispatcher _dispatcher = new();
    private readonly Dictionary<int, NativeNode> _nodes = [];

    /// <summary>
    /// Gets the dispatcher used to serialize renderer work onto the AppKit main thread.
    /// </summary>
    public override Dispatcher Dispatcher => _dispatcher;

    /// <summary>
    /// Occurs when an unhandled exception is raised while rendering.
    /// </summary>
    public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

    internal ViewAdapterResolver AdapterResolver { get; } =
        serviceProvider.GetService<ViewAdapterResolver>()
        ?? ViewAdapterResolver.CreateDefault(serviceProvider);

    internal IReadOnlyDictionary<int, NativeNode> Nodes => _nodes;

    /// <summary>
    /// Mounts a native-aware Blazor root component into an AppKit view.
    /// </summary>
    /// <typeparam name="TComponent">The root component type to instantiate.</typeparam>
    /// <param name="host">The AppKit view that will host the rendered component.</param>
    /// <returns>The renderer-assigned component identifier.</returns>
    public Task<int> MountRootComponentAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        NSView host)
        where TComponent : IComponent =>
        MountRootComponentAsync<TComponent>(host, ParameterView.Empty);

    /// <summary>
    /// Mounts a native-aware Blazor root component into an AppKit view with parameters.
    /// </summary>
    /// <typeparam name="TComponent">The root component type to instantiate.</typeparam>
    /// <param name="host">The AppKit view that will host the rendered component.</param>
    /// <param name="parameters">The parameters supplied to the root component.</param>
    /// <returns>The renderer-assigned component identifier.</returns>
    public Task<int> MountRootComponentAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        NSView host,
        ParameterView parameters)
        where TComponent : IComponent
    {
        ArgumentNullException.ThrowIfNull(host);

        return Dispatcher.InvokeAsync(async () =>
        {
            var component = InstantiateComponent(typeof(TComponent));
            var componentId = AssignRootComponentId(component);
            NativeNode node;

            if (component is NSViewControllerComponent controllerComponent)
            {
                var adapter = new ViewControllerAdapter(
                    controllerComponent,
                    new BlazorViewController(controllerComponent));
                node = new NativeNode(componentId, adapter, adapter);

                // A controller root is still supported by the legacy direct
                // view-mounting API. The new controller API uses the native
                // root-controller host below.
                _nodes.Add(componentId, node);
                new RootViewHost(host).SetChildren([adapter]);
            }
            else
            {
                node = new NativeNode(componentId, adapter: null, container: new RootViewHost(host));
                _nodes.Add(componentId, node);
            }

            await RenderRootComponentAsync(componentId, parameters);
            return componentId;
        });
    }

    /// <summary>
    /// Creates a native view controller that hosts a root Blazor view-controller component.
    /// </summary>
    /// <typeparam name="TComponent">The root view-controller component type.</typeparam>
    /// <returns>A native controller suitable for <c>NSWindow.ContentViewController</c>.</returns>
    public NSViewController CreateRootViewController<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>()
        where TComponent : NSViewControllerComponent =>
        CreateRootViewController<TComponent>(ParameterView.Empty);

    /// <summary>
    /// Creates a native view controller that hosts a parameterized root Blazor view-controller component.
    /// </summary>
    /// <typeparam name="TComponent">The root view-controller component type.</typeparam>
    /// <param name="parameters">The parameters supplied to the root component.</param>
    /// <returns>A native controller suitable for <c>NSWindow.ContentViewController</c>.</returns>
    public NSViewController CreateRootViewController<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        ParameterView parameters)
        where TComponent : NSViewControllerComponent
    {
        var snapshot = SnapshotParameters(parameters);
        return Dispatcher.InvokeAsync(() =>
            (NSViewController)new BlazorViewController(controller =>
                MountRootControllerComponentAsync<TComponent>(controller, snapshot)))
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc />
    protected override void HandleException(Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception in the AppKit Blazor renderer.");
        UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(exception, false));
    }

    /// <inheritdoc />
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

        return Task.CompletedTask;
    }

    private void ApplyComponentRenderTree(int componentId)
    {
        if (!_nodes.TryGetValue(componentId, out var parent) ||
            parent.Container is not IViewContainer container)
        {
            return;
        }

        var frames = GetCurrentRenderTreeFrames(componentId);
        var seen = new HashSet<int>();
        var desiredChildren = new List<NativeNode>();

        foreach (var frameIndex in EnumerateComponentFrames(frames.Array, 0, frames.Count))
        {
            var frame = frames.Array[frameIndex];
            var child = EnsureNode(frame, frames.Array, frameIndex);
            seen.Add(child.ComponentId);

            if (child.Parent is { } previousParent && !ReferenceEquals(previousParent, parent))
            {
                previousParent.Children.Remove(child);
            }

            child.Parent = parent;

            desiredChildren.Add(child);
        }

        var childrenChanged = parent.Children.Count != desiredChildren.Count;
        if (!childrenChanged)
        {
            for (var i = 0; i < desiredChildren.Count; i++)
            {
                if (parent.Children[i].ComponentId != desiredChildren[i].ComponentId)
                {
                    childrenChanged = true;
                    break;
                }
            }
        }

        for (var i = parent.Children.Count - 1; i >= 0; i--)
        {
            var child = parent.Children[i];
            if (!seen.Contains(child.ComponentId))
            {
                parent.Children.RemoveAt(i);
                DisposeSubtree(child);
            }
        }

        parent.Children.Clear();
        parent.Children.AddRange(desiredChildren);

        UpdateControllerContainment(parent, desiredChildren);

        if (childrenChanged)
        {
            container.SetChildren([.. desiredChildren.Select(child => child.Adapter!).Cast<IViewAdapter>()]);
        }
    }

    private NativeNode EnsureNode(
        RenderTreeFrame frame,
        RenderTreeFrame[] frames,
        int frameIndex)
    {
        if (_nodes.TryGetValue(frame.ComponentId, out var existing))
        {
            ApplyParameters(existing.Adapter!, frames, frameIndex + 1, frame.ComponentSubtreeLength - 1);
            return existing;
        }

        var component = GetComponentState(frame.ComponentId).Component;
        var adapter = component is NSViewControllerComponent controllerComponent
            ? new ViewControllerAdapter(controllerComponent, new BlazorViewController(controllerComponent))
            : AdapterResolver.Create(frame.ComponentType);
        var node = new NativeNode(frame.ComponentId, adapter);
        _nodes.Add(node.ComponentId, node);
        ApplyParameters(adapter, frames, frameIndex + 1, frame.ComponentSubtreeLength - 1);
        return node;
    }

    private async Task MountRootControllerComponentAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TComponent>(
        BlazorViewController viewController,
        ParameterView parameters)
        where TComponent : NSViewControllerComponent
    {
        try
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var component = InstantiateComponent(typeof(TComponent));
                var componentId = AssignRootComponentId(component);
                var adapter = new ViewControllerAdapter(component as TComponent
                    ?? throw new InvalidOperationException($"Root component '{typeof(TComponent).FullName}' was not instantiated as expected."), viewController);
                var node = new NativeNode(componentId, adapter, adapter);

                _nodes.Add(componentId, node);
                await RenderRootComponentAsync(componentId, parameters);
            });
        }
        catch (Exception exception)
        {
            HandleException(exception);
        }
    }

    private static ParameterView SnapshotParameters(ParameterView parameters)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var parameter in parameters)
        {
            values[parameter.Name] = parameter.Value;
        }

        return ParameterView.FromDictionary(values);
    }

    private static void UpdateControllerContainment(
        NativeNode parent,
        IReadOnlyList<NativeNode> children)
    {
        var controllerParent = FindNearestController(parent)?.Adapter is IViewControllerAdapter parentAdapter
            ? parentAdapter.ViewController
            : null;

        foreach (var child in children)
        {
            if (child.Adapter is IViewControllerAdapter controllerAdapter)
            {
                controllerAdapter.SetParentViewController(controllerParent);
            }
        }
    }

    private static NativeNode? FindNearestController(NativeNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            if (current.Adapter is IViewControllerAdapter)
            {
                return current;
            }
        }

        return null;
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
        IViewAdapter adapter,
        RenderTreeFrame[] frames,
        int start,
        int count)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        var end = Math.Min(start + count, frames.Length);
        for (var index = start; index < end; index++)
        {
            var frame = frames[index];
            if (frame.FrameType == RenderTreeFrameType.Attribute)
            {
                values[frame.AttributeName] = frame.AttributeValue;
            }
        }

        adapter.ApplyParameters(ParameterView.FromDictionary(values));
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
        node.Adapter?.Dispose();
    }
}
