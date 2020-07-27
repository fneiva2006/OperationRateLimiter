using System.Threading;
using System.Threading.Tasks;

namespace OperationRateLimiter
{
    public class Throttler : IThrottler
    {
        public int Period { get; private set; }
        public int NumOfRequests { get; private set; }
        public bool IsRunning { get; private set; }

        private readonly SemaphoreSlim _controlSemaphore;
        private Timer _timer;
        private readonly object _lock = new object();

        public Throttler(int numOfRequests, int period_ms)
        {
            Period = period_ms;
            NumOfRequests = numOfRequests;

            _controlSemaphore = new SemaphoreSlim(NumOfRequests, NumOfRequests);
        }

        public void Start()
        {
            if (!IsRunning)
            {
                _timer = new Timer(_ => ReleaseSemaphores(), null, Period, Period);
                IsRunning = true;
            }
        }

        public void Stop()
        {
            _timer.Dispose();
            ReleaseSemaphores();
            IsRunning = false;
        }

        public void WaitForPermission(CancellationToken? cancellationToken = null)
        {
            Start();

            Task task;
            lock (_lock)
            {
                task = cancellationToken.HasValue ? _controlSemaphore.WaitAsync(cancellationToken.Value)
                    : _controlSemaphore.WaitAsync();                
            }

            Task.WaitAll(task);
        }

        public async Task WaitForPermissionAsync(CancellationToken? cancellationToken = null)
        {
            Start();

            Task task;
            lock (_lock)
            {
                task = cancellationToken.HasValue ? _controlSemaphore.WaitAsync(cancellationToken.Value)
                    : _controlSemaphore.WaitAsync();
            }

            await task;
        }

        private void ReleaseSemaphores()
        {
            lock (_lock)
            {
                var releaseCount = NumOfRequests - _controlSemaphore.CurrentCount;

                if (releaseCount > 0)
                {
                    _controlSemaphore.Release(releaseCount);
                }
            }
        }

    }
}
