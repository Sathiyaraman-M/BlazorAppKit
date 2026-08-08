using System.Text;

namespace BlazorAppKit.Generators;

internal interface IGenerationStrategy
{
    bool AppliesTo(GenerationContext context);
    void AppendComponentMembers(StringBuilder source, GenerationContext context);
    void AppendAdapterFields(StringBuilder source, GenerationContext context);
    void AppendApplyParameterCases(StringBuilder source, GenerationContext context);
    void AppendAdapterMembers(StringBuilder source, GenerationContext context);
    void AppendDispose(StringBuilder source, GenerationContext context);
}

internal abstract class GenerationStrategy : IGenerationStrategy
{
    public abstract bool AppliesTo(GenerationContext context);
    public virtual void AppendComponentMembers(StringBuilder source, GenerationContext context) { }
    public virtual void AppendAdapterFields(StringBuilder source, GenerationContext context) { }
    public virtual void AppendApplyParameterCases(StringBuilder source, GenerationContext context) { }
    public virtual void AppendAdapterMembers(StringBuilder source, GenerationContext context) { }
    public virtual void AppendDispose(StringBuilder source, GenerationContext context) { }
}

internal sealed class PropertyGenerationStrategy : GenerationStrategy
{
    public override bool AppliesTo(GenerationContext context) => context.AllProperties.Any();

    public override void AppendComponentMembers(StringBuilder source, GenerationContext context)
    {
        foreach (var property in context.AllProperties)
        {
            var initializer = property.IsReferenceType ? " = default!;" : string.Empty;
            DocumentationUtilities.Append(source, context.Documentation, property.Symbol,
                $"Gets or sets the AppKit {property.NativeName} value.");
            source.AppendLine($"    [global::Microsoft.AspNetCore.Components.Parameter] public {property.TypeName} {property.ComponentName} {{ get; set; }}{initializer}");
        }
    }

    public override void AppendApplyParameterCases(StringBuilder source, GenerationContext context)
    {
        foreach (var property in context.AllProperties)
        {
            if (context.Control.IsValueAdapter && property.NativeName == context.Control.ValueProperty) continue;
            source.AppendLine($"                case nameof({context.Control.Name}.{property.ComponentName}): View.{property.NativeName} = parameter.Value is null ? default! : ({property.NativeTypeName})parameter.Value; break;");
        }
    }
}

internal sealed class EventGenerationStrategy : GenerationStrategy
{
    public override bool AppliesTo(GenerationContext context) => context.Events.Count > 0;

    public override void AppendComponentMembers(StringBuilder source, GenerationContext context)
    {
        foreach (var @event in context.Events)
        {
            DocumentationUtilities.Append(source, context.Documentation, @event.Symbol,
                $"Callback invoked when the AppKit {@event.NativeName} event occurs.");
            source.AppendLine($"    [global::Microsoft.AspNetCore.Components.Parameter] public {@event.CallbackType} {@event.ComponentName} {{ get; set; }}");
        }
    }

    public override void AppendAdapterFields(StringBuilder source, GenerationContext context)
    {
        foreach (var @event in context.Events)
            source.AppendLine($"    private {@event.CallbackType} _{@event.ComponentName};");
    }

    public override void AppendApplyParameterCases(StringBuilder source, GenerationContext context)
    {
        foreach (var @event in context.Events)
            source.AppendLine($"                case nameof({context.Control.Name}.{@event.ComponentName}) when parameter.Value is {@event.CallbackType} callback: _{@event.ComponentName} = callback; break;");
    }

    public override void AppendAdapterMembers(StringBuilder source, GenerationContext context)
    {
        foreach (var @event in context.Events)
        {
            source.AppendLine();
            if (@event.IsValueChanged)
            {
                source.AppendLine($"    private async void On{@event.NativeName}(object? sender, EventArgs e)");
                source.AppendLine("    {");
                source.AppendLine("        if (!_updatingValue)");
                source.AppendLine($"            await _{@event.ComponentName}.InvokeAsync(View.{context.Control.ValueProperty});");
                source.AppendLine("    }");
            }
            else if (@event.EventArgumentType is null)
                source.AppendLine($"    private async void On{@event.NativeName}(object? sender, EventArgs e) => await _{@event.ComponentName}.InvokeAsync();");
            else
                source.AppendLine($"    private async void On{@event.NativeName}(object? sender, {@event.EventArgumentType} e) => await _{@event.ComponentName}.InvokeAsync(e);");
        }
    }

    public override void AppendDispose(StringBuilder source, GenerationContext context)
    {
        foreach (var @event in context.Events)
            source.AppendLine($"        View.{@event.NativeName} -= On{@event.NativeName};");
    }
}

internal sealed class ValueAdapterGenerationStrategy : GenerationStrategy
{
    public override bool AppliesTo(GenerationContext context) => context.Control.IsValueAdapter;

    public override void AppendAdapterFields(StringBuilder source, GenerationContext context) => source.AppendLine("    private bool _updatingValue;");

    public override void AppendApplyParameterCases(StringBuilder source, GenerationContext context)
    {
        var control = context.Control;
        foreach (var property in context.AllProperties.Where(property => property.NativeName == control.ValueProperty))
        {
            source.AppendLine($"                case nameof({control.Name}.{property.ComponentName}):");
            var valueExpression = control.ValueType == "string?"
                ? "parameter.Value as string ?? string.Empty"
                : $"parameter.Value is null ? default({property.NativeTypeName}) : ({property.NativeTypeName})parameter.Value";
            source.AppendLine($"                    var value = {valueExpression};");
            source.AppendLine($"                    if (!EqualityComparer<{property.NativeTypeName}>.Default.Equals(View.{property.NativeName}, value))");
            source.AppendLine("                    {");
            source.AppendLine("                        _updatingValue = true;");
            source.AppendLine($"                        View.{property.NativeName} = value!;");
            source.AppendLine("                        _updatingValue = false;");
            source.AppendLine("                    }");
            source.AppendLine("                    break;");
        }
    }

    public override void AppendAdapterMembers(StringBuilder source, GenerationContext context)
    {
        var control = context.Control;
        source.AppendLine();
        var valueAssignment = control.ValueType == "string?" ? "value ?? string.Empty" : "value";
        source.AppendLine($"    public void SetValue({control.ValueType} value)");
        source.AppendLine("    {");
        source.AppendLine($"        var nextValue = {valueAssignment};");
        source.AppendLine($"        if (!EqualityComparer<{control.ValueType}>.Default.Equals(View.{control.ValueProperty}, nextValue))");
        source.AppendLine("        {");
        source.AppendLine("            _updatingValue = true;");
        source.AppendLine($"            View.{control.ValueProperty} = nextValue;");
        source.AppendLine("            _updatingValue = false;");
        source.AppendLine("        }");
        source.AppendLine("    }");
    }
}

internal sealed class ContainerGenerationStrategy : GenerationStrategy
{
    public override bool AppliesTo(GenerationContext context) => context.Control.IsContainer;

    public override void AppendComponentMembers(StringBuilder source, GenerationContext context)
    {
        DocumentationUtilities.Append(source, context.Documentation, null,
            "The child content rendered inside the native AppKit view.");
        source.AppendLine("    [global::Microsoft.AspNetCore.Components.Parameter] public global::Microsoft.AspNetCore.Components.RenderFragment? ChildContent { get; set; }");
    }

    public override void AppendAdapterMembers(StringBuilder source, GenerationContext context)
    {
        source.AppendLine();
        source.AppendLine("    public void SetChildren(global::System.Collections.Generic.IReadOnlyList<global::BlazorAppKit.Abstractions.IViewAdapter> children)");
        source.AppendLine("    {");
        source.AppendLine("        foreach (var child in children) child.View.RemoveFromSuperview();");
        switch (context.Control.ContainerKind)
        {
            case 1:
                source.AppendLine("        foreach (var child in children) View.AddArrangedSubview(child.View);");
                break;
            case 2:
                source.AppendLine("        var documentView = children.Count == 0 ? default(global::AppKit.NSView) : children[0].View;");
                source.AppendLine("        View.DocumentView = documentView!;");
                source.AppendLine("        if (documentView is not null)");
                source.AppendLine("        {");
                source.AppendLine("            documentView.Frame = View.Bounds;");
                source.AppendLine("            documentView.AutoresizingMask = global::AppKit.NSViewResizingMask.WidthSizable | global::AppKit.NSViewResizingMask.HeightSizable;");
                source.AppendLine("        }");
                break;
            case 3:
                source.AppendLine("        View.ContentView = children.Count == 0 ? default! : children[0].View;");
                break;
            default:
                source.AppendLine("        foreach (var child in children) View.AddSubview(child.View);");
                break;
        }
        source.AppendLine("    }");
    }
}
