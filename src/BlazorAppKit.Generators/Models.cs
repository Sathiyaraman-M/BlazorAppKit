using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

namespace BlazorAppKit.Generators;

internal sealed class ControlModel
{
    private const string NsView = "AppKit.NSView";
    private const string PropertyAttribute = "BlazorAppKit.Generation.GeneratedPropertyAttribute";
    private const string EventAttribute = "BlazorAppKit.Generation.GeneratedEventAttribute";

    private ControlModel(
        INamedTypeSymbol type,
        bool isContainer,
        int containerKind,
        bool isValueAdapter,
        string? valueType,
        string? valueProperty,
        string? valueChangedEvent,
        string? constructorExpression,
        ImmutableHashSet<string> excludes,
        Location? attributeLocation)
    {
        Type = type;
        Name = type.Name;
        IsContainer = isContainer;
        ContainerKind = containerKind;
        IsValueAdapter = isValueAdapter;
        ValueType = valueType;
        ValueProperty = valueProperty;
        ValueChangedEvent = valueChangedEvent;
        ConstructorExpression = constructorExpression;
        Excludes = excludes;
        AttributeLocation = attributeLocation;
    }

    public INamedTypeSymbol Type { get; }
    public string Name { get; }
    public bool IsContainer { get; }
    public int ContainerKind { get; }
    public bool IsValueAdapter { get; }
    public string? ValueType { get; }
    public string? ValueProperty { get; }
    public string? ValueChangedEvent { get; }
    public string? ConstructorExpression { get; }
    public ImmutableHashSet<string> Excludes { get; }
    public Location? AttributeLocation { get; }
    public string TypeName => SymbolUtilities.TypeName(Type);

    public static ControlModel? Create(AttributeData attribute, Compilation compilation)
    {
        var viewType = compilation.GetTypeByMetadataName(NsView);
        if (attribute.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol type ||
            viewType is null ||
            !SymbolUtilities.InheritsFrom(type, viewType))
        {
            return null;
        }

        return new ControlModel(
            type,
            AttributeUtilities.GetBool(attribute, "IsContainer"),
            AttributeUtilities.GetInt(attribute, "ContainerKind"),
            AttributeUtilities.GetBool(attribute, "IsValueAdapter"),
            AttributeUtilities.GetString(attribute, "ValueType"),
            AttributeUtilities.GetString(attribute, "ValueProperty"),
            AttributeUtilities.GetString(attribute, "ValueChangedEvent"),
            AttributeUtilities.GetString(attribute, "ConstructorExpression"),
            AttributeUtilities.GetStringArray(attribute, "ExcludeProperties"),
            attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation());
    }

    public IEnumerable<PropertyModel> GetProperties()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var type = Type; type is not null; type = type.BaseType)
        {
            foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
            {
                if (!seen.Add(property.Name) || property.IsStatic || property.IsIndexer ||
                    property.SetMethod?.DeclaredAccessibility != Accessibility.Public ||
                    Excludes.Contains(property.Name) || SymbolUtilities.IsInfrastructureProperty(property.Name)) continue;

                yield return new PropertyModel(property.Name, property.Name, SymbolUtilities.TypeName(property.Type),
                    SymbolUtilities.TypeName(property.Type), property.Type.IsReferenceType);
            }
        }
    }

    public IEnumerable<PropertyModel> GetPropertyAliases(ImmutableArray<AttributeData> attributes)
    {
        foreach (var attribute in attributes.Where(a => a.AttributeClass?.ToDisplayString() == PropertyAttribute))
        {
            if (attribute.ConstructorArguments.Length < 3 ||
                attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type ||
                !SymbolEqualityComparer.Default.Equals(type, Type)) continue;

            var nativeName = (string)attribute.ConstructorArguments[1].Value!;
            var componentName = (string)attribute.ConstructorArguments[2].Value!;
            var property = Type.GetMembers(nativeName).OfType<IPropertySymbol>().FirstOrDefault(p => p.SetMethod is not null);
            if (property is not null)
            {
                yield return new PropertyModel(nativeName, componentName, SymbolUtilities.TypeName(property.Type),
                    SymbolUtilities.TypeName(property.Type), property.Type.IsReferenceType);
            }
        }
    }

    public IEnumerable<EventModel> GetEvents(ImmutableArray<AttributeData> attributes)
    {
        var configured = attributes.Where(a => a.AttributeClass?.ToDisplayString() == EventAttribute)
            .Where(a => a.ConstructorArguments.Length >= 3 && a.ConstructorArguments[0].Value is INamedTypeSymbol type &&
                        SymbolEqualityComparer.Default.Equals(type, Type))
            .ToDictionary(a => (string)a.ConstructorArguments[1].Value!, a => a);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var type = Type; type is not null; type = type.BaseType)
        {
            foreach (var @event in type.GetMembers().OfType<IEventSymbol>())
            {
                if (!seen.Add(@event.Name) || @event.IsStatic ||
                    !SymbolUtilities.TryGetEventArgument(@event.Type, out var argumentType)) continue;

                var configuredEvent = configured.GetValueOrDefault(@event.Name);
                var callbackName = configuredEvent is null ? @event.Name : (string)configuredEvent.ConstructorArguments[2].Value!;
                var isValueChanged = configuredEvent is not null && AttributeUtilities.GetBool(configuredEvent, "IsValueChanged");
                var callbackType = isValueChanged
                    ? $"global::Microsoft.AspNetCore.Components.EventCallback<{ValueType}>"
                    : argumentType is null
                        ? "global::Microsoft.AspNetCore.Components.EventCallback"
                        : $"global::Microsoft.AspNetCore.Components.EventCallback<{argumentType}>";
                yield return new EventModel(@event.Name, callbackName, callbackType, argumentType, isValueChanged);
            }
        }
    }
}

internal sealed record PropertyModel(string NativeName, string ComponentName, string TypeName, string NativeTypeName, bool IsReferenceType);
internal sealed record EventModel(string NativeName, string ComponentName, string CallbackType, string? EventArgumentType, bool IsValueChanged);

internal sealed class GenerationContext
{
    private GenerationContext(ControlModel control, IEnumerable<PropertyModel> properties, IEnumerable<PropertyModel> aliases, IEnumerable<EventModel> events)
    {
        Control = control;
        Properties = properties.ToList();
        Aliases = aliases.ToList();
        Events = events.ToList();
    }

    public ControlModel Control { get; }
    public IReadOnlyList<PropertyModel> Properties { get; }
    public IReadOnlyList<PropertyModel> Aliases { get; }
    public IReadOnlyList<EventModel> Events { get; }
    public IEnumerable<PropertyModel> AllProperties => Properties.Concat(Aliases);

    public static GenerationContext Create(ControlModel control, ImmutableArray<AttributeData> attributes) =>
        new(control, control.GetProperties(), control.GetPropertyAliases(attributes), control.GetEvents(attributes));
}
