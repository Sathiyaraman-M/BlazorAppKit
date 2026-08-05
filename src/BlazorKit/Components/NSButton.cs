using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorKit.Components;

public sealed class NSButton : ComponentBase
{
    [Parameter] public string? Title { get; set; }
    [Parameter] public NSBezelStyle BezelStyle { get; set; }
    [Parameter] public NSCellStateValue State { get; set; }
    [Parameter] public EventCallback Activated { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) { }
}

internal sealed class NSButtonAdapter : ViewAdapter<AppKit.NSButton>
{
    private EventCallback _activated;

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
