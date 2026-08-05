using BlazorKit.Components.Base;

using Microsoft.AspNetCore.Components;

namespace BlazorKit.Components;

public sealed class Label : ComponentBase
{
    [Parameter] public string? Text { get; set; }
}

internal sealed class LabelAdapter : NativeViewAdapter<AppKit.NSTextField>
{
    public LabelAdapter()
        : base(new AppKit.NSTextField(CGRect.Empty))
    {
        View.Editable = false;
        View.Selectable = false;
        View.Bordered = false;
        View.DrawsBackground = false;
    }

    public override void ApplyParameters(ParameterView parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Name == nameof(Label.Text))
                View.StringValue = parameter.Value as string ?? string.Empty;
        }
    }
}
