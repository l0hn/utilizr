using System;
using System.Threading;

namespace Utilizr.Async
{
    public static class Sleeper
    {
        private static ManualResetEvent _waitHandle = new ManualResetEvent(false);

        static object PERMA_LOCK = new object();

        static Sleeper()
        {
            Monitor.Enter(PERMA_LOCK);
        }

        public static void Sleep(int milliseconds)
        {
            try
            {
                bool acquired = Monitor.TryEnter(PERMA_LOCK, milliseconds);

                if (acquired)
                {
                    // Monitor.Enter will not block if calling on the same thread.
                    // Enter can be called many times, but the same number of Exits
                    // will need to be invoked before other threads will not block.
                    // Not that this applies here, however.
                    _waitHandle.WaitOne(milliseconds, false);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
