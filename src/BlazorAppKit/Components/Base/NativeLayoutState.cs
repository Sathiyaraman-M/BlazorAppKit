using AppKit;

namespace BlazorAppKit.Components.Base;

/// <summary>
/// Retained native layout state owned by a view container adapter.
/// </summary>
internal sealed class NativeLayoutState : IDisposable
{
    private readonly List<NSLayoutConstraint> _constraints = [];

    public IReadOnlyList<global::AppKit.NSView> AttachedChildren { get; private set; } = [];

    public global::AppKit.NSView? HostView { get; set; }

    public bool SplitPositionInitialized { get; set; }

    public void SetAttachedChildren(IEnumerable<global::AppKit.NSView> children) => AttachedChildren = children.ToArray();

    public void ReplaceConstraints(IEnumerable<NSLayoutConstraint> constraints)
    {
        NSLayoutConstraint.DeactivateConstraints([.. _constraints]);
        _constraints.Clear();
        _constraints.AddRange(constraints);
        NSLayoutConstraint.ActivateConstraints([.. _constraints]);
    }

    public void ClearConstraints()
    {
        NSLayoutConstraint.DeactivateConstraints([.. _constraints]);
        _constraints.Clear();
    }

    public void Dispose()
    {
        ClearConstraints();
        if (HostView is not null)
        {
            HostView.RemoveFromSuperview();
            HostView.Dispose();
            HostView = null;
        }

        AttachedChildren = [];
    }
}
