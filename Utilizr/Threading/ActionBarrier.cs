

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class ActionBarrier {
    SemaphoreSlim _running;
    string _description;

    public ActionBarrier(string description = "")
    {
        _description = $"action barrier [{description}]: ";
        _running = new SemaphoreSlim(1, 1);
    }

    public async Task<bool> TryRunAsync(Func<Task> action) {
        try
        {
            await RunAsync(action);
            return true;
        }
        catch
        {
        }
        return false;
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
            await action.Invoke();
            Debug.WriteLine(_description + "completed");
        } 
        catch (Exception ex) {
            Debug.WriteLine(_description + Environment.NewLine + ex);
            throw;
        }
        finally {
            Debug.WriteLine(_description + "releasing lock");
            _running.Release();
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