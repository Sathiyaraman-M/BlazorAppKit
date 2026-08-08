using AppKit;

using BlazorAppKit.Generation;

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

namespace BlazorAppKit.Generation;

/// <summary>
/// Describes how a generated container component places its child views.
/// </summary>
public enum GeneratedContainerKind
{
    /// <summary>
    /// Places children directly in the native view's subviews collection.
    /// </summary>
    Subviews,

    /// <summary>
    /// Places children in the native stack view's arranged subviews collection.
    /// </summary>
    ArrangedSubviews,

    /// <summary>
    /// Uses the first child as the native scroll view's document view.
    /// </summary>
    DocumentView,

    /// <summary>
    /// Uses the first child as the native box's content view.
    /// </summary>
    ContentView
}

/// <summary>
/// Requests a Blazor component and native view adapter for an AppKit view type.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GenerateComponentAttribute(Type nativeType) : Attribute
{
    /// <summary>
    /// The native AppKit view type wrapped by the generated component.
    /// </summary>
    public Type NativeType { get; } = nativeType;

    /// <summary>
    /// Optional expression used to construct the native view.
    /// </summary>
    public string? ConstructorExpression { get; set; }

    /// <summary>
    /// Indicates whether the generated component accepts child content.
    /// </summary>
    public bool IsContainer { get; set; }

    /// <summary>
    /// Specifies how child views are attached to a container.
    /// </summary>
    public GeneratedContainerKind ContainerKind { get; set; }

    /// <summary>
    /// Indicates whether the component synchronizes a native value with Blazor.
    /// </summary>
    public bool IsValueAdapter { get; set; }

    /// <summary>
    /// The native property used as the component's value.
    /// </summary>
    public string? ValueProperty { get; set; }

    /// <summary>
    /// The native event raised when the component's value changes.
    /// </summary>
    public string? ValueChangedEvent { get; set; }

    /// <summary>
    /// The Blazor value type exposed by the generated component.
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// The native properties that should not become component parameters.
    /// </summary>
    public string[] ExcludeProperties { get; set; } = [];
}

/// <summary>
/// Requests an additional component parameter mapped to a native property.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GeneratedPropertyAttribute(Type nativeType, string nativeName, string componentName) : Attribute
{
    /// <summary>
    /// The native AppKit type declaring the property.
    /// </summary>
    public Type NativeType { get; } = nativeType;

    /// <summary>
    /// The native property name.
    /// </summary>
    public string NativeName { get; } = nativeName;

    /// <summary>
    /// The generated Blazor component parameter name.
    /// </summary>
    public string ComponentName { get; } = componentName;
}

/// <summary>
/// Requests an additional component callback mapped to a native event.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class GeneratedEventAttribute(Type nativeType, string nativeName, string componentName) : Attribute
{
    /// <summary>
    /// The native AppKit type declaring the event.
    /// </summary>
    public Type NativeType { get; } = nativeType;

    /// <summary>
    /// The native event name.
    /// </summary>
    public string NativeName { get; } = nativeName;

    /// <summary>
    /// The generated Blazor callback name.
    /// </summary>
    public string ComponentName { get; } = componentName;

    /// <summary>
    /// Indicates whether the callback receives the changed value instead of event arguments.
    /// </summary>
    public bool IsValueChanged { get; set; }
}
