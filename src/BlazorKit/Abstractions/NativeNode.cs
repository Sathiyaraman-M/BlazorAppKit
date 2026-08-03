namespace BlazorKit.Abstractions;

/// <summary>
/// Retained relationship between a Blazor component and its native peer.
/// </summary>
internal sealed class NativeNode(
    int componentId,
    INativeViewAdapter? adapter,
    INativeViewContainer? container = null)
{
    public int ComponentId { get; } = componentId;

    public INativeViewAdapter? Adapter { get; } = adapter;

    public INativeViewContainer? Container { get; } = container ?? adapter as INativeViewContainer;

    public NativeNode? Parent { get; set; }

    public List<NativeNode> Children { get; } = [];
}
