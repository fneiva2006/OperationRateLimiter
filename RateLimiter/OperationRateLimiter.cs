using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiter
{
    public class OperationRateLimiter
    {
        public int NumOfOperations { get; private set; }
        public int Period { get; private set; }
        public bool IsRunning { get; set; }

        private SemaphoreSlim _controlSemaphore;
        private Timer _controlTimer;
        private readonly object _semaphoreLock;

        public OperationRateLimiter(int numOfOperations, int period_ms)
        {
            NumOfOperations = numOfOperations;
            Period = period_ms;

            _controlSemaphore = new SemaphoreSlim(numOfOperations, numOfOperations);
            _semaphoreLock = new object();
        }

        public void Start()
        {
            if(!IsRunning)
            {
                _controlTimer = new Timer( _ => 
                { 
                    lock(_semaphoreLock)
                    {
                        var releaseCount = NumOfOperations - _controlSemaphore.CurrentCount;
                        if(releaseCount > 0)
                        {
                            _controlSemaphore.Release(releaseCount);
                        }
                    }
                }
                , null, Period, Period);

                IsRunning = true;
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                _controlTimer.Dispose();
                IsRunning = false;
            }
            
        }

        public void WaitForPermission()
        {
            Start();

            Task task;

            lock(_semaphoreLock)
            {
                task = _controlSemaphore.WaitAsync();
            }

            Task.WaitAll(task);
        }

    }
}
