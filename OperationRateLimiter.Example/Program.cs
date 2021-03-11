using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OperationRateLimiter;

namespace RateLimiter
{
    public class Program
    {
        static void Main(string[] args)
        {           
            var numberOfRequestsLimit = 700;
            var periodMiliseconds = 60000;

            // Instantiates the throttler (starts working automatically)
            var throttler = new Throttler(numberOfRequestsLimit, periodMiliseconds);

            var t1 = Task1(throttler);
            var t2 = Task2(throttler);
          
            var stopwatch = Stopwatch.StartNew();

            Task.WaitAll(t1, t2);

            stopwatch.Stop();

            // Stops the throttler from working
            throttler.Stop();

            Console.WriteLine($"Total elapsed time {stopwatch.Elapsed.TotalSeconds:##.00} s");
        }

        public static async Task Task1(IThrottler throttler)
        {
            Console.WriteLine("Sync throttler...  ");
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 50; i++)
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

            for (var i = 0; i < 350; i++)
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
