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
        void WaitForPermission();
        Task WaitForPermissionAsync();
    }
}