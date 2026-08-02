using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components;

public sealed class Button : ComponentBase
{
    [Parameter] public string? Text { get; set; }

    [Parameter] public EventCallback OnClick { get; set; }
}

internal sealed class ButtonAdapter : NativeViewAdapter<NSButton>
{
    private EventCallback _onClick;

    public ButtonAdapter()
        : base(new NSButton(CGRect.Empty))
    {
        View.Activated += OnActivated;
    }

    public override void SetParameter(string name, object? value)
    {
        switch (name)
        {
            case nameof(Button.Text):
                View.Title = value as string ?? string.Empty;
                break;
            case nameof(Button.OnClick):
                if (value is EventCallback callback)
                {
                    _onClick = callback;
                }
                break;
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
