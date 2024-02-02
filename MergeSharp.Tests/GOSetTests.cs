using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests;

public class GSetTests
{
    private readonly ITestOutputHelper _output;

    public GSetTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestGSetSingle()
    {
        GSet<string> gSet = new();

        gSet.Add("a");
        gSet.Add("b");
        gSet.Add("c");

        Assert.Equal(3, gSet.Count);
        Assert.Contains("a", gSet);
        Assert.Contains("b", gSet);
        Assert.Contains("c", gSet);
    }

    [Fact]
    public void TestGSetMerge()
    {
        GSet<string> gSet1 = new();
        gSet1.Add("a");
        gSet1.Add("b");

        GSet<string> gSet2 = new();
        gSet2.Add("b");
        gSet2.Add("c");

        gSet1.Merge((GSetMsg<string>)gSet2.GetLastSynchronizedUpdate());

        Assert.Equal(3, gSet1.Count);
        Assert.Contains("a", gSet1);
        Assert.Contains("b", gSet1);
        Assert.Contains("c", gSet1);
    }
    [Fact]
    public void TestGSetMergeConflictFreeAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            GSet<string> gSet1 = new();
            gSet1.Add("a");
            gSet1.Add("b");

            GSet<string> gSet2 = new();
            gSet2.Add("c");
            gSet2.Add("d");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            gSet1.Merge((GSetMsg<string>)gSet2.GetLastSynchronizedUpdate());

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }
        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
    }
    [Fact]
    public void TestGSetMergeWithConflictsAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            GSet<string> gSet1 = new();
            gSet1.Add("a");
            gSet1.Add("b");

            GSet<string> gSet2 = new();
            gSet2.Add("b");
            gSet2.Add("c");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            gSet1.Merge((GSetMsg<string>)gSet2.GetLastSynchronizedUpdate());

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
    }

}
