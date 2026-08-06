using AppKit;

using BlazorKit.Generation;

[assembly: GenerateComponent(typeof(NSView), IsContainer = true)]
[assembly: GenerateComponent(typeof(NSButton), ExcludeProperties = new[] { "Cell" })]
[assembly: GenerateComponent(typeof(NSTextField), IsValueAdapter = true, ValueProperty = "StringValue", ValueChangedEvent = "Changed", ValueType = "string?")]
[assembly: GenerateComponent(typeof(NSStackView), IsContainer = true, ContainerKind = GeneratedContainerKind.ArrangedSubviews)]
[assembly: GenerateComponent(typeof(NSSlider), IsValueAdapter = true, ValueProperty = "DoubleValue", ValueChangedEvent = "Activated", ValueType = "double")]
[assembly: GenerateComponent(typeof(NSProgressIndicator))]
[assembly: GenerateComponent(typeof(NSScrollView), IsContainer = true, ContainerKind = GeneratedContainerKind.DocumentView)]
[assembly: GenerateComponent(typeof(NSBox), IsContainer = true, ContainerKind = GeneratedContainerKind.ContentView)]
[assembly: GenerateComponent(typeof(NSClipView), IsContainer = true, ContainerKind = GeneratedContainerKind.DocumentView)]
[assembly: GenerateComponent(typeof(NSGridView), IsContainer = true, ContainerKind = GeneratedContainerKind.Subviews)]

[assembly: GeneratedProperty(typeof(NSTextField), "Editable", "IsEditable")]
[assembly: GeneratedProperty(typeof(NSTextField), "Selectable", "IsSelectable")]
[assembly: GeneratedProperty(typeof(NSTextField), "Bordered", "IsBordered")]
[assembly: GeneratedProperty(typeof(NSProgressIndicator), "Indeterminate", "IsIndeterminate")]
[assembly: GeneratedProperty(typeof(NSSlider), "Continuous", "IsContinuous")]

[assembly: GeneratedEvent(typeof(NSTextField), "Changed", "StringValueChanged", IsValueChanged = true)]
[assembly: GeneratedEvent(typeof(NSSlider), "Activated", "DoubleValueChanged", IsValueChanged = true)]

namespace BlazorKit.Generation;

public enum GeneratedContainerKind
{
    Subviews,
    ArrangedSubviews,
    DocumentView,
    ContentView
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GenerateComponentAttribute(Type nativeType) : Attribute
{
    public Type NativeType { get; } = nativeType;
    public string? ConstructorExpression { get; set; }
    public bool IsContainer { get; set; }
    public GeneratedContainerKind ContainerKind { get; set; }
    public bool IsValueAdapter { get; set; }
    public string? ValueProperty { get; set; }
    public string? ValueChangedEvent { get; set; }
    public string? ValueType { get; set; }
    public string[] ExcludeProperties { get; set; } = [];
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GeneratedPropertyAttribute(Type nativeType, string nativeName, string componentName) : Attribute
{
    public Type NativeType { get; } = nativeType;
    public string NativeName { get; } = nativeName;
    public string ComponentName { get; } = componentName;
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GeneratedEventAttribute(Type nativeType, string nativeName, string componentName) : Attribute
{
    public Type NativeType { get; } = nativeType;
    public string NativeName { get; } = nativeName;
    public string ComponentName { get; } = componentName;
    public bool IsValueChanged { get; set; }
}
