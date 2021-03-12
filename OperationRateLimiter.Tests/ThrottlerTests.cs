using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OperationRateLimiter.Tests
{
    public class ThrottlerTests
    {
        private Throttler _throttler;

        private const int NUM_OF_OPERATIONS_PER_PERIOD = 100;
        private const int TIME_PERIOD_MS = 5000;
        private const bool HAS_UNIFORM_OPERATION_RATIO = true;

        public ThrottlerTests()
        {
            _throttler = new Throttler(NUM_OF_OPERATIONS_PER_PERIOD, TIME_PERIOD_MS, HAS_UNIFORM_OPERATION_RATIO);
        }

        [Fact]
        public void LockAndExecute_ShouldReturnTask()
        {
            var task = _throttler.LockAndExecute();

            task.ShouldBeAssignableTo<Task>();
        }

        [Fact]
        public void StartStop_ShouldSetIsRunningPropertyAccordingly()
        {
            _throttler.Stop();
            _throttler.IsRunning.ShouldBeFalse();

            _throttler.Start();
            _throttler.IsRunning.ShouldBeTrue();

            _throttler.Stop();
            _throttler.IsRunning.ShouldBeFalse();
        }

        [Fact]
        public void Start_ShouldInitMasterTimer()
        {
            _throttler.Start();
            _throttler._masterTimer.ShouldNotBeNull();
        }

        [Fact]
        public void IntervalBetweenOperations_ShouldBeEqualsTimeOperationRatio()
        {
            _throttler.IntervalBetweenOperations.ShouldBe(TIME_PERIOD_MS / NUM_OF_OPERATIONS_PER_PERIOD);
        }

        [Fact]
        public void NumOfRequests_ShouldHaveConstructorParameterValues()
        {
            _throttler.Period.ShouldBe(TIME_PERIOD_MS);
            _throttler.NumOfRequests.ShouldBe(NUM_OF_OPERATIONS_PER_PERIOD);
            _throttler.HasUniformOperationRatio.ShouldBe(HAS_UNIFORM_OPERATION_RATIO);
        }

        [Fact]
        public void ReleaseSemaphores_ShoulResetAvailableOperationCount()
        {
            _throttler.WaitForPermission();
            _throttler._masterSemaphore.CurrentCount.ShouldBe(NUM_OF_OPERATIONS_PER_PERIOD - 1);

            _throttler.ReleaseSemaphores();
            _throttler._masterSemaphore.CurrentCount.ShouldBe(NUM_OF_OPERATIONS_PER_PERIOD);
        }

        [Fact]
        public void IntervalTimerReleaseSemaphoreCallback_ShouldResetIntervalControlSemaphore()
        {
            _throttler.WaitForPermission();
            _throttler._intervalControlSemaphore.CurrentCount.ShouldBe(0);

            _throttler.IntervalTimerReleaseSemaphoreCallback(null);
            _throttler._intervalControlSemaphore.CurrentCount.ShouldBe(1);
        }

        [Fact]
        public void WaitForPermission_ShouldStartTimer()
        {
            _throttler.IsRunning.ShouldBeFalse();
            _throttler.WaitForPermission();
            _throttler.IsRunning.ShouldBeTrue();
        }

        [Fact]
        public async Task WaitForPermission_ShouldStartTimerAsync()
        {
            _throttler.IsRunning.ShouldBeFalse();
            await _throttler.WaitForPermissionAsync();
            _throttler.IsRunning.ShouldBeTrue();
        }

        [Fact]
        public async Task WaitForPermission_ShouldNotWait_IfOperationIsCancelledAsync()
        {
            const int REQUESTS_PER_PERIOD = 1;
            const int TIME_FRAME_MS = 30000;
            const int TOLERABLE_TIME_AFTER_CANCELLING_MS = 5000;

            _throttler = new Throttler(REQUESTS_PER_PERIOD, TIME_FRAME_MS, true);
            await _throttler.WaitForPermissionAsync();

            var cancellationToken = new CancellationToken(true);

            Should.CompleteIn(async () => await _throttler.WaitForPermissionAsync(cancellationToken),
                TimeSpan.FromMilliseconds(TOLERABLE_TIME_AFTER_CANCELLING_MS));
        }

        [Fact]
        public async Task WaitForPermission_ShouldThrowTaskCancelledException_IfOperationIsCancelledAsync()
        {
            const int REQUESTS_PER_PERIOD = 1;
            const int TIME_FRAME_MS = 30000;

            _throttler = new Throttler(REQUESTS_PER_PERIOD, TIME_FRAME_MS, true, true);
            await _throttler.WaitForPermissionAsync();

            var cancellationToken = new CancellationToken(true);

            Should.Throw<TaskCanceledException>(async () => 
                await _throttler.WaitForPermissionAsync(cancellationToken));
        }

        [Fact]
        public void WaitForPermission_ShouldNotWait_IfOperationIsCancelled()
        {
            const int REQUESTS_PER_PERIOD = 1;
            const int TIME_FRAME_MS = 30000;
            const int TOLERABLE_TIME_AFTER_CANCELLING_MS = 5000;

            _throttler = new Throttler(REQUESTS_PER_PERIOD, TIME_FRAME_MS, true);
            _throttler.WaitForPermission();

            var cancellationToken = new CancellationToken(true);

            Should.CompleteIn(() => _throttler.WaitForPermission(cancellationToken),
                TimeSpan.FromMilliseconds(TOLERABLE_TIME_AFTER_CANCELLING_MS));
        }
        
        [Fact]
        public void WaitForPermission_ShouldThrowTaskCancelledException_IfOperationIsCancelled()
        {
            const int REQUESTS_PER_PERIOD = 1;
            const int TIME_FRAME_MS = 30000;

            _throttler = new Throttler(REQUESTS_PER_PERIOD, TIME_FRAME_MS, true, true);
            _throttler.WaitForPermission();

            var cancellationToken = new CancellationToken(true);

            Should.Throw<TaskCanceledException>(() => _throttler.WaitForPermission(cancellationToken));
        }
    }
}
