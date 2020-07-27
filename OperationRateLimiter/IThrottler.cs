using System.Threading;
using System.Threading.Tasks;

namespace OperationRateLimiter
{
    public interface IThrottler
    {
        bool IsRunning { get; }
        int NumOfRequests { get; }
        int Period { get; }

        void Start();
        void Stop();
        void WaitForPermission(CancellationToken? cancellationToken = null);
        Task WaitForPermissionAsync(CancellationToken? cancellationToken = null);
    }
}