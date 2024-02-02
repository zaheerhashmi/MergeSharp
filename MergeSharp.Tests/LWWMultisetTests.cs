using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{
    public class LWWMultiSetTests
    {
        private readonly ITestOutputHelper _output;

        public LWWMultiSetTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void TestLWWMultiSetSingle()
        {
            LWWMultiSet<string> multiSet = new();

            multiSet.Add("a", 1);
            multiSet.Add("b", 2);
            multiSet.Remove("a", 3);
            Assert.False(multiSet.Remove("c", 4));
            multiSet.Add("c", 5);

            Assert.False(multiSet.Contains("a"));
            Assert.True(multiSet.Contains("b"));
            Assert.True(multiSet.Contains("c"));
        }

        [Fact]
        public void TestLWWMultiSetMerge()
        {
            LWWMultiSet<string> multiSet1 = new();
            multiSet1.Add("a", 1);
            multiSet1.Add("b", 2);

            LWWMultiSet<string> multiSet2 = new();
            multiSet2.Add("c", 3);
            multiSet2.Add("d", 4);
            multiSet2.Remove("c", 5);

            multiSet1.Merge((LWWMultiSetMsg<string>)multiSet2.GetLastSynchronizedUpdate());

            Assert.True(multiSet1.Contains("a")); // This line should assert True, not False
            Assert.True(multiSet1.Contains("b"));
            Assert.False(multiSet1.Contains("c"));
            Assert.True(multiSet1.Contains("d"));
        }
        [Fact]
        public void TestLWWMultiSetMergeConflictFreeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                LWWMultiSet<string> multiSet1 = new();
                multiSet1.Add("a", 1);
                multiSet1.Add("b", 2);

                LWWMultiSet<string> multiSet2 = new();
                multiSet2.Add("c", 3);
                multiSet2.Add("d", 4);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                multiSet1.Merge((LWWMultiSetMsg<string>)multiSet2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }

        [Fact]
        public void TestLWWMultiSetMergeWithConflictsAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                LWWMultiSet<string> multiSet1 = new();
                multiSet1.Add("a", 1);
                multiSet1.Add("b", 2);

                LWWMultiSet<string> multiSet2 = new();
                multiSet2.Add("b", 3);
                multiSet2.Add("c", 4);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                multiSet1.Merge((LWWMultiSetMsg<string>)multiSet2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}
