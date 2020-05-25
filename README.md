[![Nuget](https://img.shields.io/nuget/v/OperationRateLimiter?color=orange)](https://www.nuget.org/packages/OperationRateLimiter/1.0.1)
# OperationRateLimiter

Use the `Throttler` object to pause the program if a certain number of operations per time period has reached its limit. When the execution of the program is blocked, it will be released only in the next time period window, or if the `stop()`method gets called.

May be used sync or asynchronously. Each instance of the `Throttler` object is independent.

Useful for limiting the rate of requests made to API's endpoints and therefore preventing receiving 429 error message response (too many requests).

### Usage example:
```C#
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
```
