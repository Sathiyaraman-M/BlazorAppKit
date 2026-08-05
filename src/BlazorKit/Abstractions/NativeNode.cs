namespace BlazorKit.Abstractions;

/// <summary>
/// Retained relationship between a Blazor component and its native peer.
/// </summary>
internal sealed class NativeNode(
    int componentId,
    INativeAdapter? adapter,
    INativeContainer? container = null)
{
    public int ComponentId { get; } = componentId;

    public INativeAdapter? Adapter { get; } = adapter;

    public INativeContainer? Container { get; } = container ?? adapter as INativeContainer;

    public NativeNode? Parent { get; set; }

    public List<NativeNode> Children { get; } = [];
}
