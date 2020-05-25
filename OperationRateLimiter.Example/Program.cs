using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OperationRateLimiter;

namespace RateLimiter
{
    public class Program
    {
        static async Task Main(string[] args)
        {           
            var numberOfRequestsLimit = 20;
            var periodMiliseconds = 5000;

            // Instantiates the throttler (starts working automatically)
            var throttler = new Throttler(numberOfRequestsLimit, periodMiliseconds);

            var t1 = Task1(throttler);
            var t2 = Task2(throttler);

            Task.WaitAll(t1, t2);

            // Stops the throttler from working
            throttler.Stop();            
        }

        public static async Task Task1(IThrottler throttler)
        {
            Console.WriteLine("Sync throttler...  ");
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 30; i++)
            {
                // Use sync mode. Will pause when requests made in the last period have reached the limit configured
                throttler.WaitForPermission();
                await Task.Delay(100);

                Console.WriteLine($"Sync - {i}");
            }
            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:##.00} s");
        }

        public static async Task Task2(IThrottler throttler)
        {
            Console.WriteLine("Async throttler...");
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 30; i++)
            {
                // USe async mode
                await throttler.WaitForPermissionAsync();
                Console.WriteLine($"Async - {i}");
            }

            stopwatch.Stop();

            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:##.00} s");
        }
    }
}
