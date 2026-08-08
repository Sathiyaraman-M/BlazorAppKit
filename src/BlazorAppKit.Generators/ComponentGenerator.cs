using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

namespace BlazorAppKit.Generators;

[Generator]
public sealed class ComponentGenerator : IIncrementalGenerator
{
    private const string ComponentAttribute = "BlazorAppKit.Generation.GenerateComponentAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationAndAttributes = context.CompilationProvider.Select(static (compilation, _) =>
            (compilation, compilation.Assembly.GetAttributes()));

        context.RegisterSourceOutput(compilationAndAttributes, static (productionContext, source) =>
            Execute(productionContext, source.compilation, source.Item2));
    }

    private static void Execute(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<AttributeData> assemblyAttributes)
    {
        var controls = assemblyAttributes
            .Where(attribute => attribute.AttributeClass?.ToDisplayString() == ComponentAttribute)
            .Select(attribute => ControlModel.Create(attribute, compilation))
            .Where(control => control is not null)
            .Cast<ControlModel>()
            .ToList();

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var control in controls)
        {
            if (!names.Add(control.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.DuplicateControl,
                    control.AttributeLocation,
                    control.Name));
                continue;
            }

            var model = GenerationContext.Create(control, assemblyAttributes);
            context.AddSource($"{control.Name}.g.cs", ComponentSourceGenerator.Generate(model));
        }

        context.AddSource("GeneratedViewAdapterRegistrations.g.cs", RegistrationSourceGenerator.Generate(controls));
    }

    private static class Diagnostics
    {
        public static readonly DiagnosticDescriptor DuplicateControl = new(
            "BKGEN001",
            "Duplicate generated component",
            "The component '{0}' was selected more than once",
            "BlazorAppKit",
            DiagnosticSeverity.Error,
            true);
    }
}
