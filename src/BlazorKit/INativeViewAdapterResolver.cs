namespace BlazorKit;

public interface INativeViewAdapterResolver
{
    INativeViewAdapter Create(Type componentType);
}
