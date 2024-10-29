using System;
using System.Threading;
using System.Threading.Tasks;
using Utilizr.Logging;

namespace Utilizr.Util;


public class RepeatingTask: IDisposable {

    private CancellationTokenSource _cancelInterval;
    private CancellationTokenSource _cancelForDispose;

    private Task _task;
    private TimeSpan _interval;
    private ManualResetEvent _resetEvent;
    private bool _paused;
    private Func<Task> _action;

    bool _firstRunComplete;
    bool _runImmediately;

    public RepeatingTask(Func<Task> actionToRepeat, TimeSpan interval, bool runImmediately = false, bool startPaused = false)
    {
        _paused = startPaused;
        _resetEvent = new ManualResetEvent(!startPaused);
        _interval = interval;
        _cancelInterval = new CancellationTokenSource();   
        _cancelForDispose = new CancellationTokenSource();
        _action = actionToRepeat;   
        _runImmediately = runImmediately;
        _task = Task.Factory.StartNew(Loop);
    }   

    public async Task RunNow() {
        var wasPaused = _paused;
        if (!_paused)
        {
            Pause();
        }
        ResetInterval();     
        try
        {        
            await _action.Invoke();
        }
        catch (System.Exception ex)
        {   
            Log.Exception(ex);
        }
        if (!wasPaused)
        {
            Resume();
        }
    }

    private async void Loop() {
        while (!_cancelForDispose.IsCancellationRequested)
        {
            try
            {
                _resetEvent.WaitOne();
                _cancelInterval.Dispose();
                _cancelInterval = new CancellationTokenSource();
                _resetEvent.WaitOne();

                if (_firstRunComplete || !_runImmediately) {
                    await Task.Delay(_interval, _cancelInterval.Token);
                }
            }
            catch (System.Exception ex)
            {
                continue;
            }   

            try
            {
                _firstRunComplete = true;
                await _action.Invoke();
            }
            catch (System.Exception ex)
            {
                Log.Exception(ex);
            } 
        }   
    }

    public void Pause() {
        _paused = true;
        _resetEvent.Reset();
    }

    public void Resume() {
        _paused = false;
        _resetEvent.Set();
    }

    public void ResetInterval() {
        _cancelInterval.Cancel();
    }

    public void ResetInterval(TimeSpan interval) {
        _interval = interval;
        _cancelInterval.Cancel();
    }

    public void Dispose()
    {
        try
        {
            _cancelForDispose.Cancel();
            _cancelInterval.Cancel();
            _resetEvent.Dispose();
            _task.Dispose();
            _cancelForDispose.Dispose();
            _cancelInterval.Dispose();
        }
        catch (System.Exception ex)
        {
            Log.Exception(ex);
        }
    }
}