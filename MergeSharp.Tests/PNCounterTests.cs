using Xunit;
using MergeSharp;
using System.Diagnostics;
using Xunit.Abstractions;
namespace MergeSharp.Tests;

public class PNCounterTests
{
    private readonly ITestOutputHelper _output;

    public PNCounterTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void TestPNCSingle()
    {
        PNCounter pnc = new PNCounter();
        pnc.Increment(5);
        pnc.Decrement(8);
        pnc.Increment(10);
        pnc.Decrement(3);

        Assert.Equal(pnc.Get(), 4);

    }

    [Fact]
    public void TestPNCMerge()
    {
        PNCounter pnc1 = new PNCounter();
        PNCounter pnc2 = new PNCounter();

        pnc1.Increment(5);
        pnc1.Decrement(8);
        pnc1.Increment(10);
        pnc1.Decrement(3);

        PNCounterMsg update = (PNCounterMsg) pnc1.GetLastSynchronizedUpdate();
        pnc2.Merge(update);
        
        Assert.Equal(pnc1.Get(), pnc2.Get());


    }
    [Fact]
    public void TestPNCMergeConflictFreeAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            PNCounter pnc1 = new PNCounter();
            pnc1.Increment(5);
            pnc1.Decrement(8);

            PNCounter pnc2 = new PNCounter();
            pnc2.Increment(10);
            pnc2.Decrement(3);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            PNCounterMsg update = (PNCounterMsg)pnc1.GetLastSynchronizedUpdate();
            pnc2.Merge(update);

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
    }

    [Fact]
    public void TestPNCMergeWithConflictsAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            PNCounter pnc1 = new PNCounter();
            pnc1.Increment(5);
            pnc1.Decrement(8);

            PNCounter pnc2 = new PNCounter();
            pnc2.Increment(5);
            pnc2.Decrement(8);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            PNCounterMsg update = (PNCounterMsg)pnc1.GetLastSynchronizedUpdate();
            pnc2.Merge(update);

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
    }




}