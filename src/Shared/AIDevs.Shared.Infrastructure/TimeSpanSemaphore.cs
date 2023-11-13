using System.Collections;
using System.Diagnostics;

namespace AIDevs.Shared.Infrastructure
{
    /// <summary>
    /// Allows a limited number of acquisitions during a timespan
    /// </summary>
    public sealed class TimeSpanSemaphore : IDisposable
    {
        private readonly SemaphoreSlim _pool;

        // the time span for the max number of callers
        private readonly TimeSpan _resetSpan;

        // keep track of the release times
        private readonly Queue<DateTime> _releaseTimes;

        // protect release time queue
        private readonly object _queueLock = new object();

        public TimeSpanSemaphore(int maxCount, TimeSpan resetSpan)
        {
            _pool = new SemaphoreSlim(maxCount, maxCount);
            _resetSpan = resetSpan;

            // initialize queue with old timestamps
            _releaseTimes = new Queue<DateTime>(maxCount);
            for (var i = 0; i < maxCount; i++)
            {
                _releaseTimes.Enqueue(DateTime.MinValue);
            }
        }

        /// <summary>
        /// Blocks the current thread until it can enter the semaphore, while observing a CancellationToken
        /// </summary>
        private async Task Wait()
        {
            // will throw if token is cancelled
            _pool.Wait();

            // get the oldest release from the queue
            DateTime oldestRelease;
            lock (_queueLock)
            {
                oldestRelease = _releaseTimes.Dequeue();
            }

            // sleep until the time since the previous release equals the reset period
            var now = DateTime.UtcNow;
            var windowReset = oldestRelease.Add(_resetSpan);
            if (windowReset > now)
            {
                var sleepMilliseconds = Math.Max(
                    (int)(windowReset.Subtract(now).Ticks / TimeSpan.TicksPerMillisecond),
                    1); // sleep at least 1ms to be sure next window has started
                Debug.WriteLine("Waiting {0} ms for TimeSpanSemaphore limit to reset.", sleepMilliseconds);

                await Task.Delay(sleepMilliseconds);
            }
        }

        /// <summary>
        /// Exits the semaphore
        /// </summary>
        private void Release()
        {
            lock (_queueLock)
            {
                _releaseTimes.Enqueue(DateTime.UtcNow);
            }
            _pool.Release();
        }

        /// <summary>
        /// Runs an action after entering the semaphore (if the CancellationToken is not canceled)
        /// </summary>
        public async Task Run(Func<Task> action)
        {
            // will throw if token is cancelled, but will auto-release lock
            await Wait();

            try
            {
                await action();
            }
            finally
            {
                Release();
            }
        }

        /// <summary>
        /// Runs an action after entering the semaphore (if the CancellationToken is not canceled)
        /// </summary>
        public async Task<T> Run<T>(Func<Task<T>> action)
        {
            await Wait();

            try
            {
                return await action();
            }
            finally
            {
                Release();
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance
        /// </summary>
        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
