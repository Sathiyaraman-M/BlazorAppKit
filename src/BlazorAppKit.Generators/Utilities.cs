using Microsoft.CodeAnalysis;

using System.Collections.Immutable;
using System.Text;
using System.Xml;
using System.Xml.Linq;

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

internal sealed class DocumentationLookup
{
    private readonly IReadOnlyDictionary<string, AdditionalText> _documentationFiles;
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _documentation = new(StringComparer.Ordinal);

    private DocumentationLookup(IReadOnlyDictionary<string, AdditionalText> documentationFiles)
    {
        _documentationFiles = documentationFiles;
    }

    public static DocumentationLookup Create(ImmutableArray<AdditionalText> additionalTexts)
    {
        var documentationFiles = additionalTexts
            .Where(text => string.Equals(GetExtension(text.Path), ".xml", StringComparison.OrdinalIgnoreCase))
            .Select(text => new
            {
                AssemblyName = GetFileNameWithoutExtension(text.Path),
                Text = text
            })
            .Where(item => item.AssemblyName is not null)
            .Select(item => (AssemblyName: item.AssemblyName!, item.Text))
            .GroupBy(item => item.AssemblyName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First().Text, StringComparer.Ordinal);

        return new DocumentationLookup(documentationFiles);
    }

    public string? Get(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml(expandIncludes: true);
        if (!string.IsNullOrWhiteSpace(xml)) return xml;

        var assemblyName = symbol.ContainingAssembly?.Name;
        var documentationId = symbol.GetDocumentationCommentId();
        if (assemblyName is null || documentationId is null ||
            !_documentationFiles.TryGetValue(assemblyName, out var documentationFile)) return null;

        if (!_documentation.TryGetValue(documentationFile.Path, out var members))
        {
            members = Load(documentationFile);
            _documentation[documentationFile.Path] = members;
        }

        return members.TryGetValue(documentationId, out var member) ? member : null;
    }

    private static IReadOnlyDictionary<string, string> Load(AdditionalText documentationFile)
    {
        try
        {
            var text = documentationFile.GetText();
            if (text is null) return ImmutableDictionary<string, string>.Empty;

            return XDocument.Parse(text.ToString())
                .Descendants("member")
                .Where(member => member.Attribute("name") is not null)
                .GroupBy(member => member.Attribute("name")!.Value, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.First().ToString(SaveOptions.DisableFormatting),
                    StringComparer.Ordinal);
        }
        catch (XmlException)
        {
            return ImmutableDictionary<string, string>.Empty;
        }
    }

    private static string? GetFileNameWithoutExtension(string path)
    {
        var fileName = path.Replace('\\', '/');
        var slash = fileName.LastIndexOf('/');
        fileName = slash >= 0 ? fileName[(slash + 1)..] : fileName;
        var extension = fileName.LastIndexOf('.');
        return extension > 0 ? fileName[..extension] : null;
    }

    private static string GetExtension(string path)
    {
        var fileName = path.Replace('\\', '/');
        var extension = fileName.LastIndexOf('.');
        return extension >= 0 ? fileName[extension..] : string.Empty;
    }
}

internal static class DocumentationUtilities
{
    public static void Append(
        StringBuilder source,
        DocumentationLookup documentation,
        ISymbol? symbol,
        string fallback,
        int indentation = 4)
    {
        IReadOnlyList<string> lines = symbol is null ? [] : GetLines(documentation.Get(symbol));
        if (lines.Count == 0)
        {
            lines = [$"<summary>{EscapeXml(fallback)}</summary>"];
        }

        var prefix = new string(' ', indentation);
        foreach (var line in lines)
        {
            source.Append(prefix).Append("///");
            if (line.Length > 0) source.Append(' ').Append(line);
            source.AppendLine();
        }
    }

    private static IReadOnlyList<string> GetLines(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return [];

        try
        {
            var root = XElement.Parse(xml, LoadOptions.PreserveWhitespace);
            var body = root.Name.LocalName switch
            {
                "member" => root,
                "doc" or "members" => root.Descendants("member").FirstOrDefault(),
                _ => root
            };

            if (body is null) return [];
            if (body == root && root.Name.LocalName is not ("member" or "doc" or "members"))
            {
                return [root.ToString(SaveOptions.DisableFormatting)];
            }

            return body.Nodes()
                .Select(node => node switch
                {
                    XElement element => element.ToString(SaveOptions.DisableFormatting),
                    XText text when !string.IsNullOrWhiteSpace(text.Value) => text.Value.Trim(),
                    _ => string.Empty
                })
                .Where(line => line.Length > 0)
                .ToArray();
        }
        catch (XmlException)
        {
            return [];
        }
    }

    private static string EscapeXml(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);
}
