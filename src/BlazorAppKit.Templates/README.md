# BlazorAppKit project templates

Install the templates with:

```bash
dotnet new install BlazorAppKit.Templates
```

Create a native macOS BlazorAppKit application with:

```bash
dotnet new blazorappkit -n MyApp
cd MyApp
dotnet run
```

> [!NOTE]
> Requires macOS workload to be installed. Run `dotnet workload install macos` to install it.

The template targets .NET 10 and macOS 15 or later. For specifying the library version explicitly, use `--blazorappkit-version` like below:

```bash
dotnet new blazorappkit -n MyApp --blazorappkit-version 0.1.0
```
