


using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Utilizr;
using Utilizr.Threading;

namespace Tests;

public class ActionBarrierTests {

    [Test]
    public async Task BarrierTest() {
        ActionBarrier barrier = new ActionBarrier("test");

        int counter = 0;

        var func = async (bool wait) => {
            counter++;
            if (!wait)
            {
                return;   
            }
            await Task.Delay(Timeout.Infinite);
        };

        _ = barrier.RunAsync(async () => await func(true));

        var allowedRun = await barrier.TryRunAsync(async () => await func(false));

        Assert.IsFalse(allowedRun);
        Assert.AreEqual(1, counter);
    }
}