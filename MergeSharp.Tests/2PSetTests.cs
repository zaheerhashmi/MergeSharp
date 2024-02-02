using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{ 

    public class TPSetTests
    {
        private readonly ITestOutputHelper _output;

        public TPSetTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void TestTPsetSingle()
        {
            TPSet<string> set = new();

            set.Add("a");
            set.Add("b");
            set.Remove("a");
            Assert.False(set.Remove("c")); // this remove should have no effect
            set.Add("c");

            Assert.Equal(2, set.Count);

            Assert.Equal(set.LookupAll(), new List<string> { "b", "c" });


        }

        [Fact]
        public void TestTPsetMerge()
        {
            TPSet<string> set = new();
            set.Add("a");
            set.Add("b");

            TPSet<string> set2 = new();
            set2.Add("c");
            set2.Add("d");
            set2.Remove("c");

            set.Merge((TPSetMsg<string>)set2.GetLastSynchronizedUpdate());

            Assert.Equal(set.LookupAll(), new List<string> { "a", "b", "d" });



        }

        [Fact]
        public void TestTPSetMergeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                TPSet<string> set1 = new();
                set1.Add("a");
                set1.Add("b");

                TPSet<string> set2 = new();
                set2.Add("c");
                set2.Add("d");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                set1.Merge((TPSetMsg<string>)set2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }

        [Fact]
        public void TestTPSetMergeAverageTimeWithConflicts()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                TPSet<string> set1 = new();
                set1.Add("a");
                set1.Add("b");

                TPSet<string> set2 = new();
                set2.Add("a");
                set2.Add("b");
                set2.Remove("a");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                set1.Merge((TPSetMsg<string>)set2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }





    }
}