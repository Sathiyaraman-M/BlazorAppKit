using AppKit;

using System.Runtime.InteropServices;

namespace BlazorAppKit.Components;

/// <summary>
/// Describes the semantic layout behavior of a native view in its parent
/// container.
/// </summary>
public enum NSViewLayoutKind
{
    /// <summary>
    /// Let the native container size the view from its intrinsic content size.
    /// </summary>
    Intrinsic,

    /// <summary>
    /// Make the view fill the available space in its parent container.
    /// </summary>
    Fill,

    /// <summary>
    /// Make the view absorb flexible space in a stack view.
    /// </summary>
    Spacer
}

/// <summary>
/// AppKit-aligned layout intent consumed by BlazorAppKit container adapters.
/// </summary>
public readonly record struct NSViewLayoutOptions
{
    /// <summary>
    /// Gets the primary semantic layout behavior.
    /// </summary>
    public NSViewLayoutKind Kind { get; init; }

    /// <summary>
    /// Gets the edge inset applied when the parent pins this view.
    /// </summary>
    public NSEdgeInsets Insets { get; init; }

    /// <summary>
    /// Gets the preferred width used by containers such as <see cref="NSSplitView"/>.
    /// </summary>
    public NFloat? PreferredWidth { get; init; }

    /// <summary>
    /// Gets the minimum width used by containers such as <see cref="NSSplitView"/>.
    /// </summary>
    public NFloat? MinimumWidth { get; init; }

    /// <summary>
    /// Gets the maximum width used by containers such as <see cref="NSSplitView"/>.
    /// </summary>
    public NFloat? MaximumWidth { get; init; }

    /// <summary>
    /// Gets the preferred height used by horizontal split containers.
    /// </summary>
    public NFloat? PreferredHeight { get; init; }

    /// <summary>
    /// Gets the minimum height used by horizontal split containers.
    /// </summary>
    public NFloat? MinimumHeight { get; init; }

    /// <summary>
    /// Gets the maximum height used by horizontal split containers.
    /// </summary>
    public NFloat? MaximumHeight { get; init; }

    /// <summary>
    /// Gets the horizontal content hugging priority override.
    /// </summary>
    public float? HorizontalHuggingPriority { get; init; }

    /// <summary>
    /// Gets the vertical content hugging priority override.
    /// </summary>
    public float? VerticalHuggingPriority { get; init; }

    /// <summary>
    /// Gets the horizontal content compression resistance priority override.
    /// </summary>
    public float? HorizontalCompressionResistancePriority { get; init; }

    /// <summary>
    /// Gets the vertical content compression resistance priority override.
    /// </summary>
    public float? VerticalCompressionResistancePriority { get; init; }

    /// <summary>
    /// Gets intrinsic sizing semantics.
    /// </summary>
    public static NSViewLayoutOptions Intrinsic => new()
    {
        Kind = NSViewLayoutKind.Intrinsic
    };

    /// <summary>
    /// Gets fill-parent semantics.
    /// </summary>
    public static NSViewLayoutOptions Fill => new()
    {
        Kind = NSViewLayoutKind.Fill
    };

    /// <summary>
    /// Gets flexible spacer semantics for stack views.
    /// </summary>
    public static NSViewLayoutOptions Spacer => new()
    {
        Kind = NSViewLayoutKind.Spacer,
        HorizontalHuggingPriority = (float)NSLayoutPriority.DefaultLow,
        VerticalHuggingPriority = (float)NSLayoutPriority.DefaultLow,
        HorizontalCompressionResistancePriority = (float)NSLayoutPriority.FittingSizeCompression,
        VerticalCompressionResistancePriority = (float)NSLayoutPriority.FittingSizeCompression
    };

    /// <summary>
    /// Returns these options with the supplied edge insets.
    /// </summary>
    public NSViewLayoutOptions WithInsets(NSEdgeInsets insets) => this with { Insets = insets };

    /// <summary>
    /// Returns these options with a preferred width and optional bounds.
    /// </summary>
    public NSViewLayoutOptions WithWidth(
        NFloat? preferredWidth,
        NFloat? minimumWidth = null,
        NFloat? maximumWidth = null) => this with
        {
            PreferredWidth = preferredWidth,
            MinimumWidth = minimumWidth,
            MaximumWidth = maximumWidth
        };

    /// <summary>
    /// Returns these options with a preferred height and optional bounds.
    /// </summary>
    public NSViewLayoutOptions WithHeight(
        NFloat? preferredHeight,
        NFloat? minimumHeight = null,
        NFloat? maximumHeight = null) => this with
        {
            PreferredHeight = preferredHeight,
            MinimumHeight = minimumHeight,
            MaximumHeight = maximumHeight
        };
}
