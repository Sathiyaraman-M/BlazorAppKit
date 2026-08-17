namespace BlazorAppKit.Abstractions;

internal interface IViewControllerAdapter
{
    NSViewController ViewController { get; }

    void SetParentViewController(NSViewController? parent);
}
