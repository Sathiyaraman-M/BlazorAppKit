namespace BlazorKit;

public interface INativeAdapterResolver
{
    INativeAdapter Create(Type componentType);
}
