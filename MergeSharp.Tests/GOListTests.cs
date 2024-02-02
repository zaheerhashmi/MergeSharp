using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{

    public class GListTests
    {
        private readonly ITestOutputHelper _output;

        public GListTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestGListSingle()
        {
            GList<string> gList = new();

            gList.Add("a");
            gList.Add("b");
            gList.Add("c");

            Assert.Equal(gList.LookupAll(), new List<string> { "a", "b", "c" });
        }

        [Fact]
        public void TestGListMerge()
        {
            GList<string> gList1 = new();
            gList1.Add("a");
            gList1.Add("b");

            GList<string> gList2 = new();
            gList2.Add("c");
            gList2.Add("d");

            gList1.Merge((GListMsg<string>)gList2.GetLastSynchronizedUpdate());

            Assert.Equal(gList1.LookupAll(), new List<string> { "a", "b", "c", "d" });
        }
        [Fact]
        public void TestGListMergeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                GList<string> gList1 = new();
                gList1.Add("a");
                gList1.Add("b");

                GList<string> gList2 = new();
                gList2.Add("c");
                gList2.Add("d");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                gList1.Merge((GListMsg<string>)gList2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }
        [Fact]
        public void TestGListMergeAverageTimeWithConflicts()
        {
            int num_operations = 100;
            double total_time = 0;
            for (int i = 0; i < num_operations; i++)
            {
                GList<string> gList1 = new();
                gList1.Add("a");
                gList1.Add("b");

                GList<string> gList2 = new();
                gList2.Add("b");
                gList2.Add("c");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                gList1.Merge((GListMsg<string>)gList2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}