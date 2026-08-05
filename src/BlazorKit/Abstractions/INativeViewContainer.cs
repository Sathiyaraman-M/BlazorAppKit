namespace BlazorKit.Abstractions;

internal interface INativeViewContainer
{
    void SetChildren(IReadOnlyList<INativeViewAdapter> children);
}

internal interface INativeContainer
{
    void SetChildren(IReadOnlyList<INativeAdapter> children);
}

internal interface INativeValueAdapter<T>
{
    void SetValue(T value);
}
