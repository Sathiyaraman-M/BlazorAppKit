using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

public sealed class NSProgressIndicator : ComponentBase
{
    [Parameter] public double DoubleValue { get; set; }
    [Parameter] public double MinValue { get; set; }
    [Parameter] public double MaxValue { get; set; } = 100;
    [Parameter] public bool IsIndeterminate { get; set; }
    [Parameter] public NSProgressIndicatorStyle Style { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) { }
}

internal sealed class NSProgressIndicatorAdapter : NativeAdapter<AppKit.NSProgressIndicator>
{
    public NSProgressIndicatorAdapter() : base(new AppKit.NSProgressIndicator(CGRect.Empty)) { }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSProgressIndicator.DoubleValue) when parameter.Value is double value: View.DoubleValue = value; break;
                case nameof(NSProgressIndicator.MinValue) when parameter.Value is double value: View.MinValue = value; break;
                case nameof(NSProgressIndicator.MaxValue) when parameter.Value is double value: View.MaxValue = value; break;
                case nameof(NSProgressIndicator.IsIndeterminate) when parameter.Value is bool value: View.Indeterminate = value; break;
                case nameof(NSProgressIndicator.Style) when parameter.Value is NSProgressIndicatorStyle style: View.Style = style; break;
            }
        }
    }
}
