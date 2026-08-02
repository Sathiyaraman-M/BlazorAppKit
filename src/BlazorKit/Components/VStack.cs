using System.Runtime.InteropServices;

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

internal sealed class VStackAdapter : NativeViewAdapter<NSStackView>, INativeViewContainerAdapter
{
    public VStackAdapter()
        : base(new NSStackView(CGRect.Empty))
    {
        View.Orientation = NSUserInterfaceLayoutOrientation.Vertical;
    }

    public override void SetParameter(string name, object? value)
    {
        if (name == nameof(VStack.Spacing) && value is double spacing)
        {
            View.Spacing = (NFloat)spacing;
        }
    }

    public void AddChild(INativeViewAdapter child) => View.AddArrangedSubview(child.View);

    public void RemoveChild(INativeViewAdapter child)
    {
        View.RemoveArrangedSubview(child.View);
        child.View.RemoveFromSuperview();
    }
}
