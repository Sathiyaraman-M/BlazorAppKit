namespace BlazorKit;

/// <summary>
/// Retained relationship between a Blazor component and its native peer.
/// </summary>
internal sealed class NativeNode(int componentId, INativeAdapter adapter)
{
    public int ComponentId { get; } = componentId;

    public INativeAdapter Adapter { get; } = adapter;

    public NativeNode? Parent { get; set; }

    public List<NativeNode> Children { get; } = [];
}
