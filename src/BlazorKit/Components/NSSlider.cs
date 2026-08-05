using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

public sealed class NSSlider : ComponentBase
{
    [Parameter] public double DoubleValue { get; set; }
    [Parameter] public EventCallback<double> DoubleValueChanged { get; set; }
    [Parameter] public double MinValue { get; set; }
    [Parameter] public double MaxValue { get; set; } = 1;
    [Parameter] public bool IsContinuous { get; set; } = true;

    protected override void BuildRenderTree(RenderTreeBuilder builder) { }
}

internal sealed class NSSliderAdapter : ViewAdapter<AppKit.NSSlider>, IValueAdapter<double>
{
    private EventCallback<double> _valueChanged;
    private bool _updating;

    public NSSliderAdapter() : base(new AppKit.NSSlider(CGRect.Empty)) => View.Activated += OnActivated;

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSSlider.DoubleValue) when parameter.Value is double value: SetValue(value); break;
                case nameof(NSSlider.DoubleValueChanged) when parameter.Value is EventCallback<double> callback: _valueChanged = callback; break;
                case nameof(NSSlider.MinValue) when parameter.Value is double value: View.MinValue = value; break;
                case nameof(NSSlider.MaxValue) when parameter.Value is double value: View.MaxValue = value; break;
                case nameof(NSSlider.IsContinuous) when parameter.Value is bool value: View.Continuous = value; break;
            }
        }
    }

    public void SetValue(double value)
    {
        if (View.DoubleValue == value) return;
        _updating = true;
        View.DoubleValue = value;
        _updating = false;
    }

    private async void OnActivated(object? sender, EventArgs e)
    {
        if (!_updating) await _valueChanged.InvokeAsync(View.DoubleValue);
    }

    public override void Dispose()
    {
        View.Activated -= OnActivated;
        base.Dispose();
    }
}
