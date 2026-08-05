namespace BlazorKit.Abstractions;

internal interface INativeContainer
{
    void SetChildren(IReadOnlyList<INativeAdapter> children);
}

internal interface INativeValueAdapter<T>
{
    void SetValue(T value);
}
