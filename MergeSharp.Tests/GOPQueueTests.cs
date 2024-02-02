using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests;

public class GPriorityQueueTests
{
    private readonly ITestOutputHelper _output;

    public GPriorityQueueTests(ITestOutputHelper output)
    {
        _output = output;
    }
    [Fact]
    public void TestGPriorityQueueSingle()
    {
        GPriorityQueue<string> gPriorityQueue = new();

        gPriorityQueue.Add(1, "a");
        gPriorityQueue.Add(3, "b");
        gPriorityQueue.Add(2, "c");

        Assert.Equal(new List<string> { "a", "c", "b" }, new List<string>(gPriorityQueue.GetElementsInPriorityOrder()));
    }

    [Fact]
    public void TestGPriorityQueueMerge()
    {
        GPriorityQueue<string> gPriorityQueue1 = new();
        gPriorityQueue1.Add(1, "a");
        gPriorityQueue1.Add(3, "b");

        GPriorityQueue<string> gPriorityQueue2 = new();
        gPriorityQueue2.Add(2, "c");
        gPriorityQueue2.Add(4, "d");
        gPriorityQueue1.Merge((GPriorityQueueMsg<string>)gPriorityQueue2.GetLastSynchronizedUpdate());

        Assert.Equal(new List<string> { "a", "c", "b", "d" }, new List<string>(gPriorityQueue1.GetElementsInPriorityOrder()));
    }
    [Fact]
    public void TestGPriorityQueueMergeConflictFreeAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            GPriorityQueue<string> gPriorityQueue1 = new();
            gPriorityQueue1.Add(1, "a");
            gPriorityQueue1.Add(3, "b");

            GPriorityQueue<string> gPriorityQueue2 = new();
            gPriorityQueue2.Add(5, "c");
            gPriorityQueue2.Add(7, "d");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            gPriorityQueue1.Merge((GPriorityQueueMsg<string>)gPriorityQueue2.GetLastSynchronizedUpdate());

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
    }

    [Fact]
    public void TestGPriorityQueueMergeWithConflictsAverageTime()
    {
        int num_operations = 100;
        double total_time = 0;

        for (int i = 0; i < num_operations; i++)
        {
            GPriorityQueue<string> gPriorityQueue1 = new();
            gPriorityQueue1.Add(1, "a");
            gPriorityQueue1.Add(3, "b");

            GPriorityQueue<string> gPriorityQueue2 = new();
            gPriorityQueue2.Add(3, "c");
            gPriorityQueue2.Add(5, "d");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            gPriorityQueue1.Merge((GPriorityQueueMsg<string>)gPriorityQueue2.GetLastSynchronizedUpdate());

            stopwatch.Stop();
            total_time += stopwatch.Elapsed.TotalMilliseconds;
        }

        double average_time = total_time / num_operations;
        _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
    }
}
