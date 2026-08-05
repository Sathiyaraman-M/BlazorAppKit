namespace BlazorKit.Abstractions;

/// <summary>
/// Retained relationship between a Blazor component and its native peer.
/// </summary>
internal sealed class NativeNode(
    int componentId,
    IViewAdapter? adapter,
    IViewContainer? container = null)
{
    public int ComponentId { get; } = componentId;

    public IViewAdapter? Adapter { get; } = adapter;

    public IViewContainer? Container { get; } = container ?? adapter as IViewContainer;

    public NativeNode? Parent { get; set; }

    public List<NativeNode> Children { get; } = [];
}
