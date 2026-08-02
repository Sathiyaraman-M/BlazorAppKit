using Microsoft.AspNetCore.Components;

using System.Runtime.InteropServices;

namespace BlazorKit;

internal abstract class NativeViewAdapter<TView>(TView view) : INativeAdapter<TView>
    where TView : NSView
{
    public TView View { get; } = view;

    NSView INativeViewAdapter.View => View;
    NSObject INativeAdapter.NativeObject => View;

    public abstract void SetParameter(string name, object? value);

    public virtual void Dispose()
    {
        View.RemoveFromSuperview();
        View.Dispose();
    }
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

internal sealed class VStackAdapter : NativeViewAdapter<NSStackView>, INativeViewContainerAdapter
{
    public VStackAdapter()
        : base(new NSStackView(CGRect.Empty))
    {
        View.Orientation = NSUserInterfaceLayoutOrientation.Vertical;
    }

    public override void SetParameter(string name, object? value)
    {
        if (name == nameof(VStack.Spacing) && value is double spacing)
        {
            View.Spacing = (NFloat)spacing;
        }
    }

    public void AddChild(INativeViewAdapter child)
    {
        View.AddArrangedSubview(child.View);
    }

    public void RemoveChild(INativeViewAdapter child)
    {
        View.RemoveArrangedSubview(child.View);
        child.View.RemoveFromSuperview();
    }

}
