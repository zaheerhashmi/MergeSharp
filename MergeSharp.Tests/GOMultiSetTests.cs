using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests;

public class GMultisetTests
{
    private readonly ITestOutputHelper _output;

    public GMultisetTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TestGMultisetSingle()
    {
        GMultiset<string> gMultiset = new();

        gMultiset.Add("a");
        gMultiset.Add("b");
        gMultiset.Add("a");

        Assert.Equal(2, gMultiset.Count("a"));
        Assert.Equal(1, gMultiset.Count("b"));
        Assert.Equal(0, gMultiset.Count("c"));
    }

    [Fact]
    public void TestGMultisetMerge()
    {
        GMultiset<string> gMultiset1 = new();
        gMultiset1.Add("a");
        gMultiset1.Add("b");
        gMultiset1.Add("b");

        GMultiset<string> gMultiset2 = new();
        gMultiset2.Add("b");
        gMultiset2.Add("c");

        gMultiset1.Merge((GMultisetMsg<string>)gMultiset2.GetLastSynchronizedUpdate());

        Assert.Equal(1, gMultiset1.Count("a"));
        Assert.Equal(3, gMultiset1.Count("b")); // Expect the merged count to be 3
        Assert.Equal(1, gMultiset1.Count("c"));
    }
    [Fact]
    public void TestGMultisetMergeAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            GMultiset<string> gMultiset1 = new();
            gMultiset1.Add("a");
            gMultiset1.Add("b");
            gMultiset1.Add("b");

            GMultiset<string> gMultiset2 = new();
            gMultiset2.Add("c");
            gMultiset2.Add("d");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            gMultiset1.Merge((GMultisetMsg<string>)gMultiset2.GetLastSynchronizedUpdate());

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
    }

    [Fact]
    public void TestGMultisetMergeAverageTimeWithConflicts()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            GMultiset<string> gMultiset1 = new();
            gMultiset1.Add("a");
            gMultiset1.Add("b");
            gMultiset1.Add("b");

            GMultiset<string> gMultiset2 = new();
            gMultiset2.Add("b");
            gMultiset2.Add("c");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            gMultiset1.Merge((GMultisetMsg<string>)gMultiset2.GetLastSynchronizedUpdate());

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
    }

}