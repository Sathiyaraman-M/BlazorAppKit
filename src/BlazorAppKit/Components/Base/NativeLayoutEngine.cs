using AppKit;

using BlazorAppKit.Abstractions;
using BlazorAppKit.Components;
using BlazorAppKit.Generation;

using System.Runtime.InteropServices;

namespace BlazorAppKit.Components.Base;

/// <summary>
/// Applies the AppKit layout recipe for each supported generated container.
/// </summary>
internal static class NativeLayoutEngine
{
    public static void Update(
        global::AppKit.NSView container,
        NativeLayoutState state,
        GeneratedContainerKind kind,
        IReadOnlyList<IViewAdapter> children)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(children);

        switch (kind)
        {
            case GeneratedContainerKind.ArrangedSubviews:
                UpdateStack((global::AppKit.NSStackView)container, state, children);
                break;
            case GeneratedContainerKind.DocumentView:
                UpdateDocumentView((global::AppKit.NSScrollView)container, state, children);
                break;
            case GeneratedContainerKind.ContentView:
                UpdateBox((global::AppKit.NSBox)container, state, children);
                break;
            case GeneratedContainerKind.SplitView:
                UpdateSplitView((global::AppKit.NSSplitView)container, state, children);
                break;
            case GeneratedContainerKind.Subviews:
            default:
                UpdateSubviews(container, state, children);
                break;
        }
    }

    public static void UpdateRoot(
        global::AppKit.NSView container,
        NativeLayoutState state,
        IReadOnlyList<IViewAdapter> children)
    {
        UpdateSubviews(container, state, children, defaultToFill: true);
    }

    private static void UpdateSubviews(
        global::AppKit.NSView container,
        NativeLayoutState state,
        IReadOnlyList<IViewAdapter> children,
        bool defaultToFill = false)
    {
        var desired = children.Select(child => child.View).ToHashSet();
        foreach (var existing in state.AttachedChildren)
        {
            if (!desired.Contains(existing))
            {
                existing.RemoveFromSuperview();
            }
        }

        var constraints = new List<NSLayoutConstraint>();
        foreach (var child in children)
        {
            if (child.View.Superview != container)
            {
                child.View.RemoveFromSuperview();
                container.AddSubview(child.View);
            }

            var layout = child.Layout;
            if (defaultToFill || layout.Kind == NSViewLayoutKind.Fill)
            {
                child.View.TranslatesAutoresizingMaskIntoConstraints = false;
                constraints.AddRange(PinEdges(container, child.View, layout.Insets));
            }
            else
            {
                child.View.TranslatesAutoresizingMaskIntoConstraints = true;
                child.View.AutoresizingMask = (NSViewResizingMask)0;
                ApplyPriorities(child.View, layout);
            }
        }

        state.ReplaceConstraints(constraints);
        state.SetAttachedChildren(children.Select(child => child.View));
    }

    private static void UpdateStack(
        global::AppKit.NSStackView container,
        NativeLayoutState state,
        IReadOnlyList<IViewAdapter> children)
    {
        var desired = children.Select(child => child.View).ToHashSet();
        foreach (var existing in state.AttachedChildren)
        {
            if (!desired.Contains(existing))
            {
                container.RemoveArrangedSubview(existing);
                existing.RemoveFromSuperview();
            }
        }

        var current = container.ArrangedSubviews;
        var orderMatches = current.Length == children.Count;
        if (orderMatches)
        {
            for (var index = 0; index < children.Count; index++)
            {
                if (!ReferenceEquals(current[index], children[index].View))
                {
                    orderMatches = false;
                    break;
                }
            }
        }

        if (!orderMatches)
        {
            foreach (var existing in current)
            {
                container.RemoveArrangedSubview(existing);
                existing.RemoveFromSuperview();
            }
        }

        foreach (var child in children)
        {
            child.View.TranslatesAutoresizingMaskIntoConstraints = false;
            ApplyPriorities(child.View, child.Layout);

            if (child.View.Superview != container || !orderMatches)
            {
                child.View.RemoveFromSuperview();
                container.AddArrangedSubview(child.View);
            }
        }

        state.ClearConstraints();
        state.SetAttachedChildren(children.Select(child => child.View));
    }

    private static void UpdateDocumentView(
        global::AppKit.NSScrollView container,
        NativeLayoutState state,
        IReadOnlyList<IViewAdapter> children)
    {
        var documentView = children.Count == 0 ? null : children[0].View;
        if (!ReferenceEquals(container.DocumentView, documentView))
        {
            container.DocumentView = documentView;
        }

        state.SetAttachedChildren(documentView is null ? [] : [documentView]);
        state.ClearConstraints();

        if (documentView is null || container.ContentView is null)
        {
            return;
        }

        documentView.TranslatesAutoresizingMaskIntoConstraints = false;
        var contentView = container.ContentView;
        state.ReplaceConstraints(
        [
            documentView.LeadingAnchor.ConstraintEqualTo(contentView.LeadingAnchor),
            documentView.TrailingAnchor.ConstraintEqualTo(contentView.TrailingAnchor),
            documentView.TopAnchor.ConstraintEqualTo(contentView.TopAnchor),
            documentView.BottomAnchor.ConstraintEqualTo(contentView.BottomAnchor),
            documentView.WidthAnchor.ConstraintEqualTo(contentView.WidthAnchor)
        ]);
    }

    private static void UpdateBox(
        global::AppKit.NSBox container,
        NativeLayoutState state,
        IReadOnlyList<IViewAdapter> children)
    {
        var child = children.Count == 0 ? null : children[0].View;
        var host = state.HostView;
        if (host is null)
        {
            host = new global::AppKit.NSView
            {
                TranslatesAutoresizingMaskIntoConstraints = true,
                AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
            };
            state.HostView = host;
        }

        if (!ReferenceEquals(container.ContentView, host))
        {
            container.ContentView = host;
        }

        foreach (var existing in host.Subviews)
        {
            if (!ReferenceEquals(existing, child))
            {
                existing.RemoveFromSuperview();
            }
        }

        state.ClearConstraints();
        if (child is null)
        {
            state.SetAttachedChildren([]);
            return;
        }

        child.RemoveFromSuperview();
        host.AddSubview(child);
        child.TranslatesAutoresizingMaskIntoConstraints = false;
        state.ReplaceConstraints(PinEdges(host, child, children[0].Layout.Insets));
        state.SetAttachedChildren([child]);
    }

    private static void UpdateSplitView(
        global::AppKit.NSSplitView container,
        NativeLayoutState state,
        IReadOnlyList<IViewAdapter> children)
    {
        var desired = children.Select(child => child.View).ToHashSet();
        foreach (var existing in state.AttachedChildren)
        {
            if (!desired.Contains(existing))
            {
                container.RemoveArrangedSubview(existing);
                existing.RemoveFromSuperview();
            }
        }

        var current = container.ArrangedSubviews;
        var orderMatches = current.Length == children.Count;
        if (orderMatches)
        {
            for (var index = 0; index < children.Count; index++)
            {
                if (!ReferenceEquals(current[index], children[index].View))
                {
                    orderMatches = false;
                    break;
                }
            }
        }

        if (!orderMatches)
        {
            foreach (var existing in current)
            {
                container.RemoveArrangedSubview(existing);
                existing.RemoveFromSuperview();
            }
        }

        foreach (var child in children)
        {
            child.View.TranslatesAutoresizingMaskIntoConstraints = true;
            child.View.AutoresizingMask = container.IsVertical
                ? NSViewResizingMask.HeightSizable
                : NSViewResizingMask.WidthSizable;

            if (child.View.Superview != container || !orderMatches)
            {
                child.View.RemoveFromSuperview();
                container.AddArrangedSubview(child.View);
            }
        }

        state.ClearConstraints();
        state.SetAttachedChildren(children.Select(child => child.View));

        if (!state.SplitPositionInitialized && children.Count > 1)
        {
            var preferred = container.IsVertical
                ? children[0].Layout.PreferredWidth
                : children[0].Layout.PreferredHeight;
            if (preferred is { } position)
            {
                container.SetPositionOfDivider(position, IntPtr.Zero);
                state.SplitPositionInitialized = true;
            }
        }
    }

    private static IEnumerable<NSLayoutConstraint> PinEdges(
        global::AppKit.NSView parent,
        global::AppKit.NSView child,
        NSEdgeInsets insets)
    {
        yield return child.LeadingAnchor.ConstraintEqualTo(parent.LeadingAnchor, insets.Left);
        yield return child.TrailingAnchor.ConstraintEqualTo(parent.TrailingAnchor, -insets.Right);
        yield return child.TopAnchor.ConstraintEqualTo(parent.TopAnchor, insets.Top);
        yield return child.BottomAnchor.ConstraintEqualTo(parent.BottomAnchor, -insets.Bottom);
    }

    private static void ApplyPriorities(global::AppKit.NSView view, NSViewLayoutOptions layout)
    {
        if (layout.HorizontalHuggingPriority is { } horizontalHugging)
        {
            view.SetContentHuggingPriorityForOrientation(horizontalHugging, NSLayoutConstraintOrientation.Horizontal);
        }

        if (layout.VerticalHuggingPriority is { } verticalHugging)
        {
            view.SetContentHuggingPriorityForOrientation(verticalHugging, NSLayoutConstraintOrientation.Vertical);
        }

        if (layout.HorizontalCompressionResistancePriority is { } horizontalCompression)
        {
            view.SetContentCompressionResistancePriority(horizontalCompression, NSLayoutConstraintOrientation.Horizontal);
        }

        if (layout.VerticalCompressionResistancePriority is { } verticalCompression)
        {
            view.SetContentCompressionResistancePriority(verticalCompression, NSLayoutConstraintOrientation.Vertical);
        }
    }
}
