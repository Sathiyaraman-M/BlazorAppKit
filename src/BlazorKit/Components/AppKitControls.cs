using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using System.Runtime.InteropServices;

namespace BlazorKit.Components;

/// <summary>Blazor projection of AppKit's NSTextField.</summary>
public sealed class NSTextField : ComponentBase
{
    [Parameter] public string? StringValue { get; set; }
    [Parameter] public EventCallback<string?> StringValueChanged { get; set; }
    [Parameter] public bool IsEditable { get; set; }
    [Parameter] public bool IsSelectable { get; set; }
    [Parameter] public bool IsBordered { get; set; }
    [Parameter] public bool DrawsBackground { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) { }
}

internal sealed class NSTextFieldAdapter : NativeAdapter<AppKit.NSTextField>, INativeValueAdapter<string?>
{
    private EventCallback<string?> _stringValueChanged;
    private bool _updating;

    public AppKit.NSTextField View => NativeObject;

    public NSTextFieldAdapter() : base(new AppKit.NSTextField(CGRect.Empty))
    {
        View.Changed += OnChanged;
    }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSTextField.StringValue):
                    SetValue(parameter.Value as string);
                    break;
                case nameof(NSTextField.StringValueChanged):
                    if (parameter.Value is EventCallback<string?> callback) _stringValueChanged = callback;
                    break;
                case nameof(NSTextField.IsEditable):
                    View.Editable = parameter.Value is true;
                    break;
                case nameof(NSTextField.IsSelectable):
                    View.Selectable = parameter.Value is true;
                    break;
                case nameof(NSTextField.IsBordered):
                    View.Bordered = parameter.Value is true;
                    break;
                case nameof(NSTextField.DrawsBackground):
                    View.DrawsBackground = parameter.Value is true;
                    break;
            }
        }
    }

    public void SetValue(string? value)
    {
        var next = value ?? string.Empty;
        if (View.StringValue == next) return;
        _updating = true;
        View.StringValue = next;
        _updating = false;
    }

    private async void OnChanged(object? sender, EventArgs e)
    {
        if (!_updating) await _stringValueChanged.InvokeAsync(View.StringValue);
    }

    public override void Dispose()
    {
        View.Changed -= OnChanged;
        base.Dispose();
    }
}

/// <summary>Blazor projection of AppKit's NSButton.</summary>
public sealed class NSButton : ComponentBase
{
    [Parameter] public string? Title { get; set; }
    [Parameter] public NSBezelStyle BezelStyle { get; set; }
    [Parameter] public NSCellStateValue State { get; set; }
    [Parameter] public EventCallback Activated { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) { }
}

internal sealed class NSButtonAdapter : NativeAdapter<AppKit.NSButton>
{
    private EventCallback _activated;

    public AppKit.NSButton View => NativeObject;

    public NSButtonAdapter() : base(new AppKit.NSButton(CGRect.Empty)) => View.Activated += OnActivated;

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSButton.Title): View.Title = parameter.Value as string ?? string.Empty; break;
                case nameof(NSButton.BezelStyle) when parameter.Value is NSBezelStyle style: View.BezelStyle = style; break;
                case nameof(NSButton.State) when parameter.Value is NSCellStateValue state: View.State = state; break;
                case nameof(NSButton.Activated) when parameter.Value is EventCallback callback: _activated = callback; break;
            }
        }
    }

    private async void OnActivated(object? sender, EventArgs e) => await _activated.InvokeAsync();

    public override void Dispose()
    {
        View.Activated -= OnActivated;
        base.Dispose();
    }
}

/// <summary>Blazor projection of AppKit's NSStackView.</summary>
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
    public AppKit.NSStackView View => NativeObject;

    public NSStackViewAdapter() : base(new AppKit.NSStackView(CGRect.Empty)) { }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSStackView.Orientation) when parameter.Value is NSUserInterfaceLayoutOrientation orientation:
                    View.Orientation = orientation;
                    break;
                case nameof(NSStackView.Spacing) when parameter.Value is double spacing:
                    View.Spacing = (NFloat)spacing;
                    break;
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
            if (child.NativeObject is not NSView view)
                throw new NotSupportedException("NSStackView children must be NSView instances.");
            View.AddArrangedSubview(view);
        }
    }
}

/// <summary>Blazor projection of AppKit's NSSlider.</summary>
public sealed class NSSlider : ComponentBase
{
    [Parameter] public double DoubleValue { get; set; }
    [Parameter] public EventCallback<double> DoubleValueChanged { get; set; }
    [Parameter] public double MinValue { get; set; }
    [Parameter] public double MaxValue { get; set; } = 1;
    [Parameter] public bool IsContinuous { get; set; } = true;

    protected override void BuildRenderTree(RenderTreeBuilder builder) { }
}

internal sealed class NSSliderAdapter : NativeAdapter<AppKit.NSSlider>, INativeValueAdapter<double>
{
    private EventCallback<double> _valueChanged;
    private bool _updating;

    public AppKit.NSSlider View => NativeObject;

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

/// <summary>Blazor projection of AppKit's NSProgressIndicator.</summary>
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
    public AppKit.NSProgressIndicator View => NativeObject;

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

/// <summary>Blazor projection of AppKit's NSScrollView.</summary>
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

internal sealed class NSScrollViewAdapter : NativeAdapter<AppKit.NSScrollView>, INativeContainer
{
    public AppKit.NSScrollView View => NativeObject;

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

    public void SetChildren(IReadOnlyList<INativeAdapter> children)
    {
        var view = children.FirstOrDefault()?.NativeObject as NSView;
        View.DocumentView = view;
        if (view is not null)
        {
            view.Frame = View.Bounds;
            view.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
        }
    }
}
