using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

public sealed class NSScrollView : ComponentBase
{
    [Parameter] public bool HasVerticalScroller { get; set; }
    [Parameter] public bool HasHorizontalScroller { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is not null) builder.AddContent(0, ChildContent);
    }
}

internal sealed class NSScrollViewAdapter : ViewAdapter<AppKit.NSScrollView>, IViewContainer
{
    public NSScrollViewAdapter() : base(new AppKit.NSScrollView(CGRect.Empty)) { }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSScrollView.HasVerticalScroller) when parameter.Value is bool value: View.HasVerticalScroller = value; break;
                case nameof(NSScrollView.HasHorizontalScroller) when parameter.Value is bool value: View.HasHorizontalScroller = value; break;
            }
        }
    }

    public void SetChildren(IReadOnlyList<IViewAdapter> children)
    {
        View.DocumentView = children.FirstOrDefault()?.View;
    }
}
