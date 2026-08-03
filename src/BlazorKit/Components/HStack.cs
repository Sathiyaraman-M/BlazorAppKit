using System.Runtime.InteropServices;

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

internal sealed class HStackAdapter : NativeViewAdapter<NSStackView>, INativeViewContainer
{
    public HStackAdapter()
        : base(new NSStackView(CGRect.Empty))
    {
        View.Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
    }

    public override void SetParameter(string name, object? value)
    {
        if (name == nameof(HStack.Spacing) && value is double spacing)
        {
            View.Spacing = (NFloat)spacing;
        }
    }

    public void SetChildren(IReadOnlyList<INativeViewAdapter> children)
    {
        foreach (var child in View.ArrangedSubviews)
        {
            View.RemoveArrangedSubview(child);
            child.RemoveFromSuperview();
        }

        foreach (var child in children)
        {
            View.AddArrangedSubview(child.View);
        }
    }
}
