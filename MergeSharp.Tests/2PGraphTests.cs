
using MergeSharp.Tests;
using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{
    public class TwoPhaseGraphTests
    {
        private readonly ITestOutputHelper _output;

        public TwoPhaseGraphTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestTwoPhaseGraphSingle()
        {
            TwoPhaseGraph<string> graph = new();

            graph.AddVertex("a");
            graph.AddVertex("b");
            graph.AddVertex("c");
            graph.AddEdge("a", "b");

            Assert.True(graph.ContainsVertex("a"));
            Assert.True(graph.ContainsVertex("b"));
            Assert.True(graph.ContainsVertex("c"));
            Assert.True(graph.ContainsEdge("a", "b"));
            Assert.False(graph.ContainsEdge("a", "c"));

            graph.RemoveVertex("c");
            graph.RemoveEdge("a", "b");

            Assert.True(graph.ContainsVertex("a"));
            Assert.True(graph.ContainsVertex("b"));
            Assert.False(graph.ContainsVertex("c"));
            Assert.False(graph.ContainsEdge("a", "b"));
        }

        [Fact]
        public void TestTwoPhaseGraphMerge()
        {
            TwoPhaseGraph<string> graph1 = new();
            graph1.AddVertex("a");
            graph1.AddVertex("b");
            graph1.AddEdge("a", "b");

            TwoPhaseGraph<string> graph2 = new();
            graph2.AddVertex("c");
            graph2.AddVertex("d");
            graph2.AddEdge("c", "d");

            graph1.Merge((TwoPhaseGraphMsg<string>)graph2.GetLastSynchronizedUpdate());

            Assert.True(graph1.ContainsVertex("a"));
            Assert.True(graph1.ContainsVertex("b"));
            Assert.True(graph1.ContainsVertex("c"));
            Assert.True(graph1.ContainsVertex("d"));
            Assert.True(graph1.ContainsEdge("a", "b"));
            Assert.True(graph1.ContainsEdge("c", "d"));
        }
        [Fact]
        public void TestTwoPhaseGraphMergeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                TwoPhaseGraph<string> graph1 = new();
                graph1.AddVertex("a");
                graph1.AddVertex("b");
                graph1.AddEdge("a", "b");

                TwoPhaseGraph<string> graph2 = new();
                graph2.AddVertex("c");
                graph2.AddVertex("d");
                graph2.AddEdge("c", "d");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                graph1.Merge((TwoPhaseGraphMsg<string>)graph2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} conflict-free merge operations: {average_time} ms");
        }

        [Fact]
        public void TestTwoPhaseGraphMergeAverageTimeWithConflicts()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                TwoPhaseGraph<string> graph1 = new();
                graph1.AddVertex("a");
                graph1.AddVertex("b");
                graph1.AddEdge("a", "b");

                TwoPhaseGraph<string> graph2 = new();
                graph2.AddVertex("a");
                graph2.AddVertex("b");
                graph2.AddEdge("a", "b");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                graph1.Merge((TwoPhaseGraphMsg<string>)graph2.GetLastSynchronizedUpdate());

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}
