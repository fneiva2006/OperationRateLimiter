using System;
using System.Threading;
using System.Threading.Tasks;

namespace OperationRateLimiter
{
    public class Throttler : IThrottler
    {
        public bool IsRunning { get; private set; }
        public int IntervalBetweenOperations { get; private set; }
        public int NumOfRequests { get; private set; }
        public int Period { get; private set; }
        public bool HasUniformOperationRatio { get; }
        public bool ShouldThrowTaskCancelledException { get; }

        internal readonly SemaphoreSlim _masterSemaphore;
        internal readonly SemaphoreSlim _intervalControlSemaphore = new SemaphoreSlim(1, 1);
        
        internal Timer _masterTimer;

        private readonly object _lock = new object();

        public Throttler(int numOfRequests, int period_ms, bool hasUniformOperationRatio = true, bool shouldThrowTaskCancelledException = false)
        {
            IntervalBetweenOperations = period_ms / numOfRequests;
            NumOfRequests = numOfRequests;
            Period = period_ms;
            HasUniformOperationRatio = hasUniformOperationRatio;
            ShouldThrowTaskCancelledException = shouldThrowTaskCancelledException;

            if (HasUniformOperationRatio)
            {
                new Timer(IntervalTimerReleaseSemaphoreCallback, 
                    null, IntervalBetweenOperations, IntervalBetweenOperations);
            }

            _masterSemaphore = new SemaphoreSlim(NumOfRequests, NumOfRequests);
        }

        #region Public methods

        public void Start()
        {
            if (!IsRunning)
            {
                _masterTimer = new Timer(_ => ReleaseSemaphores(), null, Period, Period);
                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (_masterTimer != null)
            {
                _masterTimer.Dispose();
            }
            
            ReleaseSemaphores();
            IsRunning = false;
        }

        public void WaitForPermission(CancellationToken? cancellationToken = null)
        {
            Start();

            try
            {
                if (HasUniformOperationRatio)
                {
                    WaitForIntervalControlSemaphoreAsync(cancellationToken).Wait();
                }
                
                LockAndExecute(cancellationToken).Wait();
            }
            catch (AggregateException aggregateException)
            {
                if (ShouldThrowTaskCancelledException && 
                    aggregateException.InnerException is TaskCanceledException)
                {
                    throw aggregateException.InnerException;
                }
            }           
        }

        public async Task WaitForPermissionAsync(CancellationToken? cancellationToken = null)
        {
            Start();

            try
            {
                if (HasUniformOperationRatio)
                {
                    await WaitForIntervalControlSemaphoreAsync(cancellationToken);
                }
                
                await LockAndExecute(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                if (ShouldThrowTaskCancelledException)
                {
                    throw;
                }
            }            
        }

        #endregion

        #region Auxiliary methods

        internal async Task WaitForIntervalControlSemaphoreAsync(CancellationToken? cancellationToken)
        {
            if (cancellationToken.HasValue)
            {
                await _intervalControlSemaphore.WaitAsync(cancellationToken.Value);
            }
            else
            {
                await _intervalControlSemaphore.WaitAsync();
            }                
        }

        internal void IntervalTimerReleaseSemaphoreCallback(object state)
        {
            if (_intervalControlSemaphore.CurrentCount.Equals(0))
            {
                _intervalControlSemaphore.Release();
            }
        }

        internal Task LockAndExecute(CancellationToken? cancellationToken = null)
        {
            lock (_lock)
            {
                return cancellationToken.HasValue ? _masterSemaphore.WaitAsync(cancellationToken.Value)
                    : _masterSemaphore.WaitAsync();
            }
        }

        internal void ReleaseSemaphores()
        {
            lock (_lock)
            {
                var releaseCount = NumOfRequests - _masterSemaphore.CurrentCount;

                if (releaseCount > 0)
                {
                    _masterSemaphore.Release(releaseCount);
                }
            }
        }

        #endregion

    }
}
