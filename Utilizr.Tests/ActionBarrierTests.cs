
using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Utilizr.Threading;

namespace Tests;

public class ActionBarrierTests {

    [Test]
    public async Task BarrierTest() {
        ActionBarrier barrier = new ActionBarrier("test");
        var signal = new ManualResetEvent(false);

        int counter = 0;

        var func = async (bool wait) => {
            counter++;
            if (!wait)
            {
                return;   
            }
            await Task.Run(() => signal.WaitOne());
        };

        var runningTask = barrier.TryRunAsync(async () => await func(true), false);

        var result = await barrier.TryRunAsync(async () => await func(false), false);
        Assert.IsFalse(result.RanTask);
        Assert.IsNotNull(result.BlockingTask);
        Assert.IsNotNull(result.Error);

        signal.Set();

        var blockingTaskResult = await runningTask;
        Assert.IsTrue(blockingTaskResult.RanTask);
        Assert.IsNull(blockingTaskResult.BlockingTask);
        Assert.IsNull(blockingTaskResult.Error);
        Assert.AreEqual(1, counter);

        if (result.BlockingTask != null)
        {
            await result.BlockingTask;
        }
    }
}