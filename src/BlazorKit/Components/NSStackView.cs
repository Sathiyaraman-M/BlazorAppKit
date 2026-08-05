using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using System.Runtime.InteropServices;

namespace BlazorKit.Components;

public sealed class NSStackView : ComponentBase
{
    [Parameter] public NSUserInterfaceLayoutOrientation Orientation { get; set; }
    [Parameter] public double Spacing { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null) builder.AddContent(0, ChildContent);
    }
}

internal sealed class NSStackViewAdapter : NativeAdapter<AppKit.NSStackView>, INativeContainer
{
    public NSStackViewAdapter() : base(new AppKit.NSStackView(CGRect.Empty)) { }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSStackView.Orientation) when parameter.Value is NSUserInterfaceLayoutOrientation orientation: View.Orientation = orientation; break;
                case nameof(NSStackView.Spacing) when parameter.Value is double spacing: View.Spacing = (NFloat)spacing; break;
            }
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
            View.AddArrangedSubview(child.View);
        }
    }
}
