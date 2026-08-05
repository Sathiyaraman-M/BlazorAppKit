using System.Runtime.InteropServices;

using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

public sealed class HStack : ComponentBase
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

internal sealed class HStackAdapter : NativeViewAdapter<AppKit.NSStackView>, INativeContainer
{
    public HStackAdapter()
        : base(new AppKit.NSStackView(CGRect.Empty))
    {
        View.Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
    }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Name == nameof(HStack.Spacing) && parameter.Value is double spacing)
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
                throw new NotSupportedException("HStack children must be NSView instances.");
            View.AddArrangedSubview(nativeView);
        }
    }
}
