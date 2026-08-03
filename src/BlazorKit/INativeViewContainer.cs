namespace BlazorKit;

public interface INativeViewContainer
{
    void SetChildren(IReadOnlyList<INativeViewAdapter> children);
}
