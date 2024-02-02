using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{
    public class GrowOnlyMapTests
    {
        private readonly ITestOutputHelper _output;

        public GrowOnlyMapTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestGrowOnlyMapSingle()
        {
            GrowOnlyMap<string, int> map = new();
            map.Put("A", 1);
            map.Put("B", 2);
            map.Put("A", 3);

            Assert.Equal(1, map.Get("A"));
            Assert.Equal(2, map.Get("B"));
        }

        [Fact]
        public void TestGrowOnlyMapMerge()
        {
            GrowOnlyMap<string, int> map1 = new();
            map1.Put("A", 1);

            GrowOnlyMap<string, int> map2 = new();
            map2.Put("B", 2);

            map1.ApplySynchronizedUpdate(map2.GetLastSynchronizedUpdate());
            Assert.Equal(1, map1.Get("A"));
            Assert.Equal(2, map1.Get("B"));
        }
        [Fact]
        public void TestGrowOnlyMapMergeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                GrowOnlyMap<string, int> map1 = new();
                map1.Put("A", 1);

                GrowOnlyMap<string, int> map2 = new();
                map2.Put("B", 2);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                map1.ApplySynchronizedUpdate(map2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }

        [Fact]
        public void TestGrowOnlyMapMergeAverageTimeWithConflicts()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                GrowOnlyMap<string, int> map1 = new();
                map1.Put("A", 1);

                GrowOnlyMap<string, int> map2 = new();
                map2.Put("A", 2);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                map1.ApplySynchronizedUpdate(map2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}
