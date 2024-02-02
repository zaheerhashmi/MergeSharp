using Xunit;
using MergeSharp;
using System.Diagnostics;
using Xunit.Abstractions;
namespace MergeSharp.Tests
{

    public class GCounterTests
    {
        private readonly ITestOutputHelper _output;

        public GCounterTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void TestGCounterSingle()
        {
            GCounter gCounter = new GCounter();
            gCounter.Increment(5);
            gCounter.Increment(10);

            Assert.Equal(15, gCounter.Get());
        }

        [Fact]
        public void TestGCounterMerge()
        {
            GCounter gCounter1 = new GCounter();
            GCounter gCounter2 = new GCounter();

            gCounter1.Increment(5);
            gCounter1.Increment(10);

            GCounterMsg update = (GCounterMsg)gCounter1.GetLastSynchronizedUpdate();
            gCounter2.Merge(update);

            Assert.Equal(gCounter1.Get(), gCounter2.Get());
        }
        [Fact]
        public void TestGCounterMergeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                GCounter gCounter1 = new GCounter();
                gCounter1.Increment(5);

                GCounter gCounter2 = new GCounter();
                gCounter2.Increment(10);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                gCounter1.Merge((GCounterMsg)gCounter2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }

        [Fact]
        public void TestGCounterMergeAverageTimeWithConflicts()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                GCounter gCounter1 = new GCounter();
                gCounter1.Increment(5);

                GCounter gCounter2 = new GCounter();
                gCounter2.Increment(5);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                gCounter1.Merge((GCounterMsg)gCounter2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}