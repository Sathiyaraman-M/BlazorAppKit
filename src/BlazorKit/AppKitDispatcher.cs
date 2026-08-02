using Microsoft.AspNetCore.Components;

namespace BlazorKit;

internal sealed class AppKitDispatcher : Dispatcher
{
    private readonly NSObject _invoker = new();

    public override bool CheckAccess() => NSThread.IsMain;

    public override Task InvokeAsync(Action workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (CheckAccess())
        {
            try
            {
                workItem();
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        }

        var completion = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _invoker.BeginInvokeOnMainThread(() =>
        {
            try
            {
                workItem();
                completion.SetResult(null);
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        return completion.Task;
    }

    public override Task InvokeAsync(Func<Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (CheckAccess())
        {
            return workItem();
        }

        var completion = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _invoker.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await workItem().ConfigureAwait(true);
                completion.SetResult(null);
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        return completion.Task;
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (CheckAccess())
        {
            try
            {
                return Task.FromResult(workItem());
            }
            catch (Exception exception)
            {
                return Task.FromException<TResult>(exception);
            }
        }

        var completion = new TaskCompletionSource<TResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _invoker.BeginInvokeOnMainThread(() =>
        {
            try
            {
                completion.SetResult(workItem());
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        return completion.Task;
    }

    public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        if (CheckAccess())
        {
            return workItem();
        }

        var completion = new TaskCompletionSource<TResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        _invoker.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                completion.SetResult(await workItem().ConfigureAwait(true));
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });

        return completion.Task;
    }
}
