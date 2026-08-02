using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components;

public sealed class Label : ComponentBase
{
    [Parameter] public string? Text { get; set; }
}

internal sealed class LabelAdapter : NativeViewAdapter<NSTextField>
{
    public LabelAdapter()
        : base(new NSTextField(CGRect.Empty))
    {
        View.Editable = false;
        View.Selectable = false;
        View.Bordered = false;
        View.DrawsBackground = false;
    }

    public override void SetParameter(string name, object? value)
    {
        if (name == nameof(Label.Text))
        {
            View.StringValue = value as string ?? string.Empty;
        }
    }
}
