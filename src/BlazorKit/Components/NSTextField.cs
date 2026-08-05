using BlazorKit.Abstractions;
using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

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

internal sealed class NSTextFieldAdapter : ViewAdapter<AppKit.NSTextField>, IValueAdapter<string?>
{
    private EventCallback<string?> _stringValueChanged;
    private bool _updating;

    public NSTextFieldAdapter() : base(new AppKit.NSTextField(CGRect.Empty)) => View.Changed += OnChanged;

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(NSTextField.StringValue): SetValue(parameter.Value as string); break;
                case nameof(NSTextField.StringValueChanged) when parameter.Value is EventCallback<string?> callback: _stringValueChanged = callback; break;
                case nameof(NSTextField.IsEditable): View.Editable = parameter.Value is true; break;
                case nameof(NSTextField.IsSelectable): View.Selectable = parameter.Value is true; break;
                case nameof(NSTextField.IsBordered): View.Bordered = parameter.Value is true; break;
                case nameof(NSTextField.DrawsBackground): View.DrawsBackground = parameter.Value is true; break;
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
