using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Utilizr.Threading;

public class ActionBarrier
{
    SemaphoreSlim _running;
    string _description;
    Task? _currentTask;

    public ActionBarrier(string description = "")
    {
        _description = $"action barrier [{description}]: ";
        _running = new SemaphoreSlim(1, 1);
    }

    public async Task<TryRunResult> TryRunAsync(Func<Task> action, bool waitExisting = false)
    {
        var result = new TryRunResult();
        try
        {
            await RunAsync(action, waitExisting);
            result.RanTask = true;
        }
        catch (ActionBarrierAlreadyRunningException abex)
        {
            if (waitExisting)
            {
                return result;
            }
            result.BlockingTask = _currentTask;
            result.Error = abex;
        }
        catch (Exception ex)
        {
            result.Error = ex;
            result.RanTask = true;
        }
        return result;
    }

    public async Task RunAsync(Func<Task> action, bool waitExisting = false)
    {
        var result = new TryRunResult();
        try
        {
            await _run(action);
            result.RanTask = true;
        }
        catch (ActionBarrierAlreadyRunningException)
        {
            if (waitExisting)
            {
                await WaitRunningTask();
                return;
            }
            throw;
        }
    }

    private async Task _run(Func<Task> action)
    {
        if (!await _running.WaitAsync(0))
        {
            Debug.WriteLine(_description + "is already running");
            throw new ActionBarrierAlreadyRunningException("the action is already running");
        }

        Debug.WriteLine(_description + "acquired lock");

        try
        {
            Debug.WriteLine(_description + "executing");
            _currentTask = action.Invoke();
            await _currentTask;
            Debug.WriteLine(_description + "completed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(_description + Environment.NewLine + ex);
            throw;
        }
        finally
        {
            _currentTask = null;
            Debug.WriteLine(_description + "releasing lock");
            _running.Release();
        }
    }

    // Now private due to breaking change (now throws exceptions).
    // Use TryWaitRunningTask instead.
    private async Task WaitRunningTask()
    {
        if (_currentTask == null)
        {
            return;
        }
        await _currentTask;
    }


    public async Task<TryRunResult> TryWaitRunningTask()
    {
        var result = new TryRunResult();
        try
        {
            if (_currentTask == null)
            {
                return result;
            }
            result.BlockingTask = _currentTask;
            await _currentTask;
        }
        catch (Exception ex)
        {
            result.Error = ex;
        }
        return result;
    }

    public void Dispose()
    {
        if (_running != null)
        {
            _running.Dispose();
        }
    }

}

[System.Serializable]
public class ActionBarrierAlreadyRunningException : System.Exception
{
    public ActionBarrierAlreadyRunningException() { }
    public ActionBarrierAlreadyRunningException(string message) : base(message) { }
    public ActionBarrierAlreadyRunningException(string message, System.Exception inner) : base(message, inner) { }
}

public class TryRunResult
{
    public bool RanTask { get; internal set; }
    public Task? BlockingTask { get; internal set; }
    public Exception? Error { get; internal set; }

    public TryRunResult()
    {

    }
}
