

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Utilizr.Threading;

public class ActionBarrier {
    SemaphoreSlim _running;
    string _description;
    Task? _currentTask;

    public ActionBarrier(string description = "")
    {
        _description = $"action barrier [{description}]: ";
        _running = new SemaphoreSlim(1, 1);
    }

    public async Task<TryRunResult> TryRunAsync(Func<Task> action, bool waitExisting) {        
        var result = new TryRunResult();
        try
        {
            await RunAsync(action);
            result.RanTask = true;
        }
        catch (ActionBarrierException abex) {
            result.Error = abex;
            result.BlockingTask = _currentTask;
            if (waitExisting && result.BlockingTask != null)
            {
                await result.BlockingTask;
            }
        }
        catch (Exception ex){
            result.Error = ex;
            result.RanTask = true;
        }
        return result;
    }   

    public async Task RunAsync(Func<Task> action) {
        await _run(action);
    }

    private async Task _run(Func<Task> action) {
        if (! await _running.WaitAsync(0))
        {
            Debug.WriteLine(_description + "is already running");
            throw new ActionBarrierException("the action is already running");
        }

        Debug.WriteLine(_description + "acquired lock");

        try
        {
            Debug.WriteLine(_description + "executing");
            _currentTask = action.Invoke();
            await _currentTask;
            Debug.WriteLine(_description + "completed");
        } 
        catch (Exception ex) {
            Debug.WriteLine(_description + Environment.NewLine + ex);
            throw;
        }
        finally {
            _currentTask = null;
            Debug.WriteLine(_description + "releasing lock");
            _running.Release();
        }
    }

    public async Task WaitRunningTask() {
        if (_currentTask == null) 
        {
            return;   
        }
        
        try
        {
            await _currentTask;
        }
        catch (System.Exception)
        {
        }   
    }

    public void Dispose()
    {
        
    }

}

[System.Serializable]
public class ActionBarrierException : System.Exception
{
    public ActionBarrierException() { }
    public ActionBarrierException(string message) : base(message) { }
    public ActionBarrierException(string message, System.Exception inner) : base(message, inner) { }
    protected ActionBarrierException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}   

public class TryRunResult {
    public bool RanTask { get; internal set;}
    public Task? BlockingTask { get; internal set;}
    public Exception? Error { get; internal set; }

    public TryRunResult()
    {
       
    }
}