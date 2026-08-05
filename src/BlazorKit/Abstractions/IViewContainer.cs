namespace BlazorKit.Abstractions;

internal interface IViewContainer
{
    void SetChildren(IReadOnlyList<IViewAdapter> children);
}
