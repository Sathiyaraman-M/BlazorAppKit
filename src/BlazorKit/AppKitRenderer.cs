using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace BlazorKit;

public class AppKitRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : Renderer(serviceProvider, loggerFactory)
{
    public override Dispatcher Dispatcher => throw new NotImplementedException();

    protected override void HandleException(Exception exception)
    {
        throw new NotImplementedException();
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        throw new NotImplementedException();
    }
}

