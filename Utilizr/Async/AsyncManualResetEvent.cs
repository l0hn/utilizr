using System.Threading.Tasks;

namespace Utilizr.Async;

public class AsyncManualResetEvent {
    private volatile TaskCompletionSource? _tcs;

    public AsyncManualResetEvent(bool initialState) {
        if (!initialState) {
            Reset();
        }
    }

    public Task WaitAsync() {
        return _tcs?.Task ?? Task.CompletedTask;
    }

    public void Set() {
        var tcs = _tcs;
        Task.Run(() => tcs?.TrySetResult());
    }

    public void Reset() {
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
