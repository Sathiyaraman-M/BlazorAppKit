namespace BlazorKit.Abstractions;

internal interface IValueAdapter<T>
{
    void SetValue(T value);
}
