# BlazorKit project templates

Install the templates with:

```bash
dotnet new install BlazorKit.Templates
```

Create a native macOS BlazorKit application with:

```bash
dotnet new blazorkit -n MyApp
cd MyApp
dotnet run
```

> [!NOTE]
> Requires macOS workload to be installed. Run `dotnet workload install macos` to install it. 

The template targets .NET 10 and macOS 15 or later. For specifying the library version explicitly, use `--blazorkit-version` like below:

```bash
dotnet new blazorkit -n MyApp --blazorkit-version 0.1.0
```
