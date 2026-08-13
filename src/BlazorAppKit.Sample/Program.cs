using AppKit;
using CoreGraphics;
using Foundation;
using System.Runtime.InteropServices;

namespace BlazorAppKit.Sample;

internal static class Program
{
    private static void Main(string[] args)
    {
        NSApplication.Init();
        NSApplication.SharedApplication.Delegate = new SampleApplicationDelegate();
        NSApplication.Main(args);
    }
}

internal sealed class SampleApplicationDelegate : NSApplicationDelegate
{
    private NSWindow? _window;

    public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;

    public override void DidFinishLaunching(NSNotification notification)
    {
        _window = new NSWindow(
            new CGRect(0, 0, 1024, 700),
            NSWindowStyle.Titled |
            NSWindowStyle.Closable |
            NSWindowStyle.Resizable,
            NSBackingStore.Buffered,
            deferCreation: false)
        {
            Title = "BlazorAppKit Sample",
            ContentViewController = new RootViewController(),
            MinSize = new CGSize(760, 520)
        };

        _window.Center();
        _window.MakeKeyAndOrderFront(null);
    }
}

internal enum SamplePage
{
    Counter,
    Binding
}

internal sealed class SampleState
{
    public event EventHandler? Changed;

    public SamplePage CurrentPage { get; private set; } = SamplePage.Counter;
    public int CurrentCount { get; private set; }
    public string Name { get; private set; } = "Ada";
    public string SearchText { get; private set; } = string.Empty;

    public void SelectPage(SamplePage page)
    {
        if (CurrentPage == page) return;
        CurrentPage = page;
        NotifyChanged();
    }

    public void Increment() => SetCount(CurrentCount + 1);

    public void SetName(string value)
    {
        if (Name == value) return;
        Name = value;
        NotifyChanged();
    }

    public void SetSearchText(string value)
    {
        if (SearchText == value) return;
        SearchText = value;
        NotifyChanged();
    }

    private void SetCount(int value)
    {
        CurrentCount = value;
        NotifyChanged();
    }

    private void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
}

internal sealed class RootViewController : NSViewController
{
    private readonly SampleState _state = new();
    private readonly NSSplitViewController _splitViewController;

    public RootViewController()
    {
        var sidebarController = new SidebarViewController(_state);
        var contentController = new ContentViewController(_state);

        _splitViewController = new NSSplitViewController();
        AddChildViewController(_splitViewController);
        _splitViewController.SplitView.IsVertical = true;
        _splitViewController.SplitView.DividerStyle = NSSplitViewDividerStyle.Thin;

        var sidebarItem = NSSplitViewItem.CreateSidebar(sidebarController);
        sidebarItem.MinimumThickness = 220;
        sidebarItem.PreferredThicknessFraction = new NFloat(0.27);
        sidebarItem.MaximumThickness = 320;
        sidebarItem.CanCollapse = true;

        var contentItem = NSSplitViewItem.CreateContentList(contentController);

        _splitViewController.AddSplitViewItem(sidebarItem);
        _splitViewController.AddSplitViewItem(contentItem);

    }

    public override void LoadView()
    {
        // The split view is the root of the window, just like the macOS
        // Settings shell. There is no outer vertical header consuming space
        // above the sidebar.
        View = _splitViewController.View;
    }
}

internal sealed class SidebarViewController : NSViewController
{
    private readonly SampleState _state;
    private NSSearchField? _searchField;
    private NSButton? _counterButton;
    private NSButton? _bindingButton;

    public SidebarViewController(SampleState state) => _state = state;

    public override void LoadView()
    {
        _searchField = new NSSearchField { PlaceholderString = "Search" };
        _searchField.Changed += (_, _) => _state.SetSearchText(_searchField.StringValue);

        var pagesLabel = new NSTextField
        {
            StringValue = "PAGES",
            Editable = false,
            Bordered = false,
            DrawsBackground = false,
            TextColor = NSColor.SecondaryLabel,
            Font = NSFont.SystemFontOfSize(11)
        };

        _counterButton = CreatePageButton("Counter", SamplePage.Counter);
        _bindingButton = CreatePageButton("Two-way binding", SamplePage.Binding);

        var pages = new NSStackView
        {
            Orientation = NSUserInterfaceLayoutOrientation.Vertical,
            Alignment = NSLayoutAttribute.Width,
            Distribution = NSStackViewDistribution.Fill,
            Spacing = 6,
            EdgeInsets = new NSEdgeInsets(16, 16, 18, 16),
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        pages.AddArrangedSubview(_searchField);
        pages.AddArrangedSubview(pagesLabel);
        pages.AddArrangedSubview(_counterButton);
        pages.AddArrangedSubview(_bindingButton);

        var sidebar = new NSVisualEffectView
        {
            Material = NSVisualEffectMaterial.Sidebar,
            BlendingMode = NSVisualEffectBlendingMode.WithinWindow,
            State = NSVisualEffectState.Active
        };

        sidebar.AddSubview(pages);
        NSLayoutConstraint.ActivateConstraints(new[]
        {
            pages.LeadingAnchor.ConstraintEqualTo(sidebar.LeadingAnchor),
            pages.TrailingAnchor.ConstraintEqualTo(sidebar.TrailingAnchor),
            pages.TopAnchor.ConstraintEqualTo(sidebar.TopAnchor)
        });

        View = sidebar;

        _state.Changed += OnStateChanged;
        UpdateView();
    }

    private NSButton CreatePageButton(string title, SamplePage page)
    {
        var button = new NSButton
        {
            Title = title,
            Alignment = NSTextAlignment.Left,
            BezelStyle = NSBezelStyle.Recessed
        };
        button.Activated += (_, _) => _state.SelectPage(page);
        return button;
    }

    private void OnStateChanged(object? sender, EventArgs e) => UpdateView();

    private void UpdateView()
    {
        if (_searchField is null || _counterButton is null || _bindingButton is null)
            return;

        var query = _state.SearchText.Trim();
        _counterButton.Hidden = query.Length > 0 && !"counter".Contains(query, StringComparison.OrdinalIgnoreCase);
        _bindingButton.Hidden = query.Length > 0 && !"two-way binding".Contains(query, StringComparison.OrdinalIgnoreCase);

        UpdateButton(_counterButton, SamplePage.Counter, "Counter");
        UpdateButton(_bindingButton, SamplePage.Binding, "Two-way binding");
    }

    private void UpdateButton(NSButton button, SamplePage page, string title)
    {
        var selected = _state.CurrentPage == page;
        button.Title = selected ? $"●  {title}" : $"    {title}";
        button.Bordered = selected;
        button.BezelColor = selected ? NSColor.ControlAccent : null;
        button.ContentTintColor = selected ? NSColor.White : NSColor.Text;
    }
}

internal sealed class ContentViewController : NSViewController
{
    private readonly SampleState _state;
    private NSTextField? _counterTitle;
    private NSTextField? _counterDescription;
    private NSTextField? _countLabel;
    private NSButton? _incrementButton;
    private NSBox? _counterCard;
    private NSBox? _bindingCard;
    private NSTextField? _nameField;
    private NSTextField? _boundNameLabel;
    private NSButton? _backButton;
    private NSButton? _forwardButton;

    public ContentViewController(SampleState state) => _state = state;

    public override void LoadView()
    {
        var rootView = new NSView();

        var toolbarHost = new NSView
        {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        var toolbar = BuildToolbar();
        toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
        toolbarHost.AddSubview(toolbar);

        var scrollView = new NSScrollView
        {
            HasVerticalScroller = true,
            HasHorizontalScroller = false,
            BorderType = NSBorderType.NoBorder,
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        var contentView = BuildContentView();
        scrollView.DocumentView = contentView;

        // The document view tracks the visible width but derives its height
        // from the page stack, which is the AppKit scroll-view pattern for a
        // vertically scrolling settings page.
        contentView.TranslatesAutoresizingMaskIntoConstraints = false;
        NSLayoutConstraint.ActivateConstraints(new[]
        {
            contentView.LeadingAnchor.ConstraintEqualTo(scrollView.ContentView.LeadingAnchor),
            contentView.TrailingAnchor.ConstraintEqualTo(scrollView.ContentView.TrailingAnchor),
            contentView.TopAnchor.ConstraintEqualTo(scrollView.ContentView.TopAnchor),
            contentView.BottomAnchor.ConstraintEqualTo(scrollView.ContentView.BottomAnchor),
            contentView.WidthAnchor.ConstraintEqualTo(scrollView.ContentView.WidthAnchor)
        });

        rootView.AddSubview(toolbarHost);
        rootView.AddSubview(scrollView);
        NSLayoutConstraint.ActivateConstraints(new[]
        {
            toolbarHost.LeadingAnchor.ConstraintEqualTo(rootView.LeadingAnchor),
            toolbarHost.TrailingAnchor.ConstraintEqualTo(rootView.TrailingAnchor),
            toolbarHost.TopAnchor.ConstraintEqualTo(rootView.TopAnchor),
            toolbarHost.HeightAnchor.ConstraintEqualTo(new NFloat(58)),

            toolbar.LeadingAnchor.ConstraintEqualTo(toolbarHost.LeadingAnchor, 16),
            toolbar.TrailingAnchor.ConstraintLessThanOrEqualTo(toolbarHost.TrailingAnchor, 16),
            toolbar.TopAnchor.ConstraintEqualTo(toolbarHost.TopAnchor, 8),
            toolbar.BottomAnchor.ConstraintEqualTo(toolbarHost.BottomAnchor, 8),

            scrollView.LeadingAnchor.ConstraintEqualTo(rootView.LeadingAnchor),
            scrollView.TrailingAnchor.ConstraintEqualTo(rootView.TrailingAnchor),
            scrollView.TopAnchor.ConstraintEqualTo(toolbarHost.BottomAnchor),
            scrollView.BottomAnchor.ConstraintEqualTo(rootView.BottomAnchor)
        });

        View = rootView;
        _state.Changed += OnStateChanged;
        UpdateView();
    }

    private NSView BuildToolbar()
    {
        _backButton = new NSButton
        {
            Title = "‹",
            BezelStyle = NSBezelStyle.TexturedRounded,
            Bordered = false
        };
        _backButton.Activated += (_, _) => _state.SelectPage(SamplePage.Counter);

        _forwardButton = new NSButton
        {
            Title = "›",
            BezelStyle = NSBezelStyle.TexturedRounded,
            Bordered = false
        };
        _forwardButton.Activated += (_, _) => _state.SelectPage(SamplePage.Binding);

        var navigation = new NSStackView
        {
            Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
            Alignment = NSLayoutAttribute.CenterY,
            Spacing = 0
        };
        navigation.AddArrangedSubview(_backButton);
        navigation.AddArrangedSubview(_forwardButton);

        var navigationPill = new NSBox
        {
            BoxType = NSBoxType.NSBoxCustom,
            BorderType = NSBorderType.NoBorder,
            FillColor = NSColor.ControlBackground,
            CornerRadius = 14,
            ContentViewMargins = new CGSize(2, 1),
            ContentView = navigation
        };

        var toolbar = new NSStackView
        {
            Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
            Alignment = NSLayoutAttribute.CenterY,
            Spacing = 10
        };
        toolbar.AddArrangedSubview(navigationPill);
        toolbar.AddArrangedSubview(new NSTextField
        {
            StringValue = "BlazorAppKit Settings",
            Editable = false,
            Bordered = false,
            DrawsBackground = false,
            Font = NSFont.SystemFontOfSize(17)
        });

        return toolbar;
    }

    private NSView BuildContentView()
    {
        var page = new NSStackView
        {
            Orientation = NSUserInterfaceLayoutOrientation.Vertical,
            Alignment = NSLayoutAttribute.Width,
            Distribution = NSStackViewDistribution.Fill,
            Spacing = 14,
            EdgeInsets = new NSEdgeInsets(20, 28, 28, 28),
            TranslatesAutoresizingMaskIntoConstraints = false
        };

        _counterTitle = Label("Counter", 24, NSColor.Text);
        _counterDescription = Label("A small counter page hosted inside a native AppKit shell.", 0, NSColor.SecondaryLabel);
        _counterCard = BuildCard(14, new CGSize(24, 20), VerticalStack(_counterTitle, _counterDescription));
        _counterCard.SetContentHuggingPriorityForOrientation(1000, NSLayoutConstraintOrientation.Vertical);
        page.AddArrangedSubview(_counterCard);

        _countLabel = Label("Current count: 0", 0, NSColor.Text);
        _incrementButton = new NSButton
        {
            Title = "Increment",
            BezelStyle = NSBezelStyle.Rounded
        };
        _incrementButton.Activated += (_, _) => _state.Increment();
        var counterControls = HorizontalStack(_countLabel, _incrementButton);
        var counterControlsCard = BuildCard(12, new CGSize(18, 16), counterControls);
        counterControlsCard.SetContentHuggingPriorityForOrientation(1000, NSLayoutConstraintOrientation.Vertical);
        page.AddArrangedSubview(counterControlsCard);

        var bindingTitle = Label("Two-way binding", 24, NSColor.Text);
        var bindingDescription = Label("Changes in either control update the same Blazor value.", 0, NSColor.SecondaryLabel);
        _nameField = new NSTextField
        {
            Editable = true,
            Bordered = true,
            StringValue = _state.Name
        };
        _nameField.Changed += (_, _) => _state.SetName(_nameField.StringValue);
        _boundNameLabel = Label($"Bound value: {_state.Name}", 0, NSColor.SecondaryLabel);

        var bindingBody = VerticalStack(
            Label("Name", 0, NSColor.Text),
            _nameField,
            _boundNameLabel);
        _bindingCard = BuildCard(14, new CGSize(24, 20), VerticalStack(bindingTitle, bindingDescription, bindingBody));
        _bindingCard.SetContentHuggingPriorityForOrientation(1000, NSLayoutConstraintOrientation.Vertical);
        page.AddArrangedSubview(_bindingCard);

        // Keep the cards at the top when the window is tall. This spacer is
        // the flexible arranged subview; it absorbs surplus height instead
        // of stretching the actual settings cards or centering the page.
        var spacer = new NSView();
        spacer.SetContentHuggingPriorityForOrientation(1, NSLayoutConstraintOrientation.Vertical);
        page.AddArrangedSubview(spacer);

        return page;
    }

    private void OnStateChanged(object? sender, EventArgs e) => UpdateView();

    private void UpdateView()
    {
        if (_counterCard is null || _bindingCard is null || _countLabel is null || _nameField is null || _boundNameLabel is null)
            return;

        var isCounter = _state.CurrentPage == SamplePage.Counter;
        _counterCard.Hidden = !isCounter;
        _bindingCard.Hidden = isCounter;
        _countLabel.StringValue = $"Current count: {_state.CurrentCount}";
        _boundNameLabel.StringValue = $"Bound value: {_state.Name}";

        if (_nameField.StringValue != _state.Name)
            _nameField.StringValue = _state.Name;
    }

    private static NSTextField Label(string text, int size, NSColor color)
    {
        var label = new NSTextField
        {
            StringValue = text,
            Editable = false,
            Bordered = false,
            DrawsBackground = false,
            TextColor = color
        };
        if (size > 0)
            label.Font = NSFont.SystemFontOfSize(size);
        return label;
    }

    private static NSBox BuildCard(NFloat cornerRadius, CGSize margins, NSView content) => new()
    {
        BoxType = NSBoxType.NSBoxCustom,
        BorderType = NSBorderType.NoBorder,
        FillColor = NSColor.ControlBackground,
        CornerRadius = cornerRadius,
        ContentViewMargins = margins,
        ContentView = content
    };

    private static NSStackView VerticalStack(params NSView[] views) => Stack(NSUserInterfaceLayoutOrientation.Vertical, NSLayoutAttribute.Width, 8, views);

    private static NSStackView HorizontalStack(params NSView[] views) => Stack(NSUserInterfaceLayoutOrientation.Horizontal, NSLayoutAttribute.CenterY, 14, views);

    private static NSStackView Stack(NSUserInterfaceLayoutOrientation orientation, NSLayoutAttribute alignment, NFloat spacing, params NSView[] views)
    {
        var stack = new NSStackView
        {
            Orientation = orientation,
            Alignment = alignment,
            Spacing = spacing
        };
        foreach (var view in views)
            stack.AddArrangedSubview(view);
        return stack;
    }
}
