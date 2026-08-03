using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components;

/// <summary>
/// A native AppKit text field that supports Blazor two-way binding.
/// </summary>
public sealed class TextField : ComponentBase
{
    [Parameter] public string? Value { get; set; }

    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
}

internal sealed class TextFieldAdapter : NativeViewAdapter<NSTextField>
{
    private EventCallback<string?> _valueChanged;
    private bool _updatingValue;

    public TextFieldAdapter()
        : base(new NSTextField(CGRect.Empty))
    {
        View.Changed += OnChanged;
    }

    public override void SetParameter(string name, object? value)
    {
        switch (name)
        {
            case nameof(TextField.Value):
                var stringValue = value as string ?? string.Empty;
                if (View.StringValue != stringValue)
                {
                    _updatingValue = true;
                    View.StringValue = stringValue;
                    _updatingValue = false;
                }

                break;
            case nameof(TextField.ValueChanged):
                if (value is EventCallback<string?> callback)
                {
                    _valueChanged = callback;
                }

                break;
        }
    }

    private async void OnChanged(object? sender, EventArgs e)
    {
        if (!_updatingValue)
        {
            await _valueChanged.InvokeAsync(View.StringValue);
        }
    }

    public override void Dispose()
    {
        View.Changed -= OnChanged;
        base.Dispose();
    }
}
