namespace BlazorKit.Abstractions;

public interface INativeViewContainer
{
    void SetChildren(IReadOnlyList<INativeViewAdapter> children);
}
