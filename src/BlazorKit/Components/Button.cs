using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components;

public sealed class Button : ComponentBase
{
    [Parameter] public string? Text { get; set; }

    [Parameter] public EventCallback OnClick { get; set; }
}

internal sealed class ButtonAdapter : NativeViewAdapter<AppKit.NSButton>
{
    private EventCallback _onClick;

    public ButtonAdapter()
        : base(new AppKit.NSButton(CGRect.Empty))
    {
        View.Activated += OnActivated;
    }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            switch (parameter.Name)
            {
                case nameof(Button.Text): View.Title = parameter.Value as string ?? string.Empty; break;
                case nameof(Button.OnClick) when parameter.Value is EventCallback callback: _onClick = callback; break;
            }
        }
    }

    private async void OnActivated(object? sender, EventArgs e)
    {
        await _onClick.InvokeAsync();
    }

    public override void Dispose()
    {
        View.Activated -= OnActivated;
        base.Dispose();
    }
}
