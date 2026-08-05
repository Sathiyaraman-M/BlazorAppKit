using System.Runtime.InteropServices;

using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

public sealed class VStack : ComponentBase
{
    [Parameter] public double Spacing { get; set; } = 8;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null)
        {
            builder.AddContent(0, ChildContent);
        }
    }
}

internal sealed class VStackAdapter : NativeViewAdapter<AppKit.NSStackView>, INativeContainer
{
    public VStackAdapter()
        : base(new AppKit.NSStackView(CGRect.Empty))
    {
        View.Orientation = NSUserInterfaceLayoutOrientation.Vertical;
    }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Name == nameof(VStack.Spacing) && parameter.Value is double spacing)
                View.Spacing = (NFloat)spacing;
        }
    }

    public void SetChildren(IReadOnlyList<INativeAdapter> children)
    {
        foreach (var child in View.ArrangedSubviews)
        {
            View.RemoveArrangedSubview(child);
            child.RemoveFromSuperview();
        }

        foreach (var child in children)
        {
            if (child.NativeObject is not NSView nativeView)
                throw new NotSupportedException("VStack children must be NSView instances.");
            View.AddArrangedSubview(nativeView);
        }
    }
}
