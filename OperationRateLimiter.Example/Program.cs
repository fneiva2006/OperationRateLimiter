using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OperationRateLimiter;

namespace RateLimiter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var throttler = new Throttler(30, 5000);

            var t1 = Task1(throttler);
            var t2 = Task2(throttler);

            Task.WaitAll(t1, t2);
        }


        public static async Task Task1(IThrottler throttler)
        {
            Console.WriteLine("Sync throttler...  ");
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 30; i++)
            {
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
                await throttler.WaitForPermissionAsync();
                Console.WriteLine($"Async - {i}");
            }

            stopwatch.Stop();

            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:##.00} s");
        }
    }
}
