using Microsoft.AspNetCore.Components;

namespace BlazorAppKit.Components;

/// <summary>
/// Common Blazor base for generated native AppKit view components.
/// </summary>
public abstract class NSViewComponent : ComponentBase
{
    /// <summary>
    /// Gets or sets the semantic layout intent consumed by the parent native container.
    /// </summary>
    [Parameter]
    public NSViewLayoutOptions Layout { get; set; } = NSViewLayoutOptions.Intrinsic;
}
