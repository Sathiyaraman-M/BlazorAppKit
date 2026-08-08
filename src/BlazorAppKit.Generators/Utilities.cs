using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

namespace BlazorAppKit.Generators;

internal static class AttributeUtilities
{
    public static bool GetBool(AttributeData data, string name) => data.NamedArguments.FirstOrDefault(a => a.Key == name).Value.Value is true;
    public static int GetInt(AttributeData data, string name) => data.NamedArguments.FirstOrDefault(a => a.Key == name).Value.Value is int value ? value : 0;
    public static string? GetString(AttributeData data, string name) => data.NamedArguments.FirstOrDefault(a => a.Key == name).Value.Value as string;

    public static ImmutableHashSet<string> GetStringArray(AttributeData data, string name)
    {
        var argument = data.NamedArguments.FirstOrDefault(a => a.Key == name);
        if (argument.Key is null || argument.Value.Kind != TypedConstantKind.Array) return ImmutableHashSet<string>.Empty;
        return argument.Value.Values.Select(item => item.Value as string).Where(item => item is not null).Cast<string>().ToImmutableHashSet(StringComparer.Ordinal);
    }
}

internal static class SymbolUtilities
{
    public static string TypeName(ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    public static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType) => SymbolEqualityComparer.Default.Equals(type, baseType) || (type.BaseType is not null && InheritsFrom(type.BaseType, baseType));
    public static bool IsInfrastructureProperty(string name) => name is "ClassHandle" or "Handle" or "SuperHandle" or "Subviews" or "Superview" or "Window" or "WeakDelegate" or "Delegate" or "DataSource" or "ArrangedSubviews" or "ContentView" or "DocumentView";

    public static bool TryGetEventArgument(ITypeSymbol type, out string? argumentType)
    {
        argumentType = null;
        if (type.ToDisplayString() == "System.EventHandler") return true;
        if (type is INamedTypeSymbol named && named.OriginalDefinition.ToDisplayString() == "System.EventHandler<TEventArgs>")
        {
            argumentType = TypeName(named.TypeArguments[0]);
            return true;
        }
        return false;
    }

    public static string FindConstructor(INamedTypeSymbol type)
    {
        var rect = type.InstanceConstructors.FirstOrDefault(c => c.Parameters.Length == 1 && c.Parameters[0].Type.ToDisplayString() == "CoreGraphics.CGRect");
        return rect is not null ? $"new {TypeName(type)}(global::CoreGraphics.CGRect.Empty)" : $"new {TypeName(type)}()";
    }
}
