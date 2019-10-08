using BenchmarkDotNet.Running;

namespace LongestSleepFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AnalyticsService>();
        }
    }
}