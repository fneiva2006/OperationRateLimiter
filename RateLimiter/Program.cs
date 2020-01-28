using System;
using System.Diagnostics;
using RateLimiter;

namespace RateLimiter
{
    class Program
    {
        static void Main(string[] args)
        {
            var rateLimiter = new OperationRateLimiter(10, 1000);

            var stopwatch = Stopwatch.StartNew();

            for(var i=0; i<30; i++)
            {
                rateLimiter.WaitForPermission();
                Console.WriteLine($"{i}");
            }
            
            stopwatch.Stop();

            Console.WriteLine($"{stopwatch.Elapsed.TotalSeconds:##.00} s");
        }
    }
}
