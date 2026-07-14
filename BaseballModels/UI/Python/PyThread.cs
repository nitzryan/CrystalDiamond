using System.Collections.Concurrent;
using Python.Runtime;
using UI.Python;

internal static class PyThread
{
    private static readonly BlockingCollection<Action<Exception?>> _queue = new();
    private static readonly TaskCompletionSource _ready =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static Thread? _thread;
    private static Exception? _fatal;   // set if the interpreter failed to start

    public static Task Ready => _ready.Task;

    public static void Start()
    {
        if (_thread != null)
            return;

        _thread = new Thread(ThreadLoop)
        {
            Name = "Python",
            IsBackground = true
        };
        _thread.Start();
    }

    private static void ThreadLoop()
    {
        try
        {
            PyCore.Initialize();
            _ready.TrySetResult();
        }
        catch (Exception ex)
        {
            _fatal = ex;
            _ready.TrySetException(ex);
            _queue.CompleteAdding();

            foreach (var work in _queue.GetConsumingEnumerable())
                work(ex);

            return;
        }

        foreach (var work in _queue.GetConsumingEnumerable())
            work(null);

        PyCore.Shutdown();
    }

    // Do some work in Python on the Python thread
    public static Task<T> InvokeAsync<T>(Func<T> func)
    {
        if (_fatal != null)
            return Task.FromException<T>(_fatal);

        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        void Work(Exception? pumpError)
        {
            if (pumpError != null)
            {
                tcs.TrySetException(pumpError);
                return;
            }

            try
            {
                using (Py.GIL())
                    tcs.TrySetResult(func());
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }

        try
        {
            _queue.Add(Work);
        }
        catch (InvalidOperationException)   // queue closed: app is shutting down
        {
            tcs.TrySetCanceled();
        }

        return tcs.Task;
    }

    public static Task InvokeAsync(Action action) => InvokeAsync(() =>
    {
        action();
        return true;
    });

    public static void Stop()
    {
        if (_thread == null)
            return;

        _queue.CompleteAdding();
        _thread.Join();
        _thread = null;
    }
}