using System;
using System.Threading.Tasks;

namespace Utilizr.Util
{
    public class RateLimiter
    {
        DateTime _nextEventAllowedDrop = DateTime.MinValue;
        DateTime _nextEventAllowedWait = DateTime.MinValue;

        public int MinimumInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumInterval">
        /// <summary>
        /// The amount of time in milliseconds that must elapse before an action is executed
        /// </summary>
        /// </param>
        public RateLimiter(int minimumInterval)
        {
            MinimumInterval = minimumInterval;
        }

        /// <summary>
        /// Execute an action with rate limiting.
        /// NOTE: There is no guarantee that the action will be invoked. i.e. if rate limiting occurs the action will be dropped
        /// </summary>
        /// <param name="action"></param>
        public void ExecuteActionDrop(Action action)
        {
            var now = DateTime.UtcNow;
            if (_nextEventAllowedDrop > now)
                return;

            _nextEventAllowedDrop = now.AddMilliseconds(MinimumInterval);
            action();
        }

        /// <summary>
        /// Execute an action with rate limiting.
        /// NOTE: There is no guarantee that the action will be invoked. i.e. if rate limiting occurs the action will be dropped
        /// </summary>
        public Task ExecuteActionDropAsync(Action action)
        {
            return Task.Run(() => ExecuteActionDrop(action));
        }


        /// <summary>
        /// Execute an action with rate limiting.
        /// NOTE: This will block until the minimum time has passed, then allowing the action to be invoked.
        /// This should nearly always never be used, this will slow down any background processing, favour ExecuteAction wherever possible.
        /// </summary>
        /// <param name="action"></param>
        public async void ExecuteActionWait(Action action)
        {
            var now = DateTime.UtcNow;
            if (_nextEventAllowedWait > now)
            {
                // +1 to account for casting to int from double
                var toWaitMs = (int)(_nextEventAllowedWait - now).TotalMilliseconds + 1;
                await Task.Delay(toWaitMs);
            }

            _nextEventAllowedWait = now.AddMilliseconds(MinimumInterval);
            action();
        }
    }
}