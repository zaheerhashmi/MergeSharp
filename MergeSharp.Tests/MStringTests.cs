
using MergeSharp.Tests;
using System;
using Xunit;
using MergeSharp;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace MergeSharp.Tests
{
    public class MStringTests
    {
        private readonly ITestOutputHelper _output;

        public MStringTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestMStringInsertDelete()
        {
            MString MString = new();

            MString.Insert(0, 'A');
            MString.Insert(1, 'B');
            MString.Insert(2, 'C');
            MString.Insert(3, 'D');
            MString.Delete(1);

            Assert.Equal("ACD", MString.GetText());
        }

        [Fact]
        public void TestMStringMerge()
        {
            MString MString1 = new();
            MString MString2 = new();

            MString1.Insert(0, 'A');
            MString1.Insert(1, 'B');
            MString1.Insert(2, 'C');
            MString1.Insert(3, 'D');

            MString2.Insert(0, '1');
            MString2.Insert(1, '2');
            MString2.Insert(2, '3');
            MString2.Insert(3, '4');

            MStringMsg update1 = (MStringMsg)MString1.GetLastSynchronizedUpdate();
            MStringMsg update2 = (MStringMsg)MString2.GetLastSynchronizedUpdate();
            MString1.Merge(update2);
            MString2.Merge(update1);

            Console.WriteLine(MString1.GetText());
            Assert.Equal(MString2.GetText(), MString1.GetText());
        }

        [Fact]
        public void TestMStringMergeAverageTime()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                MString MString1 = new();
                MString MString2 = new();

                MString1.Insert(0, 'A');
                MString1.Insert(1, 'B');
                MString1.Insert(2, 'C');
                MString1.Insert(3, 'D');

                MString2.Insert(0, '1');
                MString2.Insert(1, '2');
                MString2.Insert(2, '3');
                MString2.Insert(3, '4');

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                MStringMsg update1 = (MStringMsg)MString1.GetLastSynchronizedUpdate();
                MStringMsg update2 = (MStringMsg)MString2.GetLastSynchronizedUpdate();
                MString1.Merge(update2);
                MString2.Merge(update1);

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations without conflicts: {average_time} ms");
        }
        [Fact]
        public void TestMStringMergeAverageTimeWithConflicts()
        {
            int num_operations = 100;
            double total_time = 0;

            for (int i = 0; i < num_operations; i++)
            {
                MString MString1 = new();
                MString MString2 = new();

                MString1.Insert(0, 'A');
                MString1.Insert(1, 'B');
                MString1.Insert(2, 'C');
                MString1.Insert(3, 'D');

                MString2.Insert(0, '1');
                MString2.Insert(1, '2');
                MString2.Insert(2, '3');
                MString2.Insert(3, '4');

                // Introduce conflicting updates
                MString1.Delete(1);
                MString2.Delete(1);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                MStringMsg update1 = (MStringMsg)MString1.GetLastSynchronizedUpdate();
                MStringMsg update2 = (MStringMsg)MString2.GetLastSynchronizedUpdate();
                MString1.Merge(update2);
                MString2.Merge(update1);

                stopwatch.Stop();
                total_time += stopwatch.Elapsed.TotalMilliseconds;
            }

            double average_time = total_time / num_operations;
            _output.WriteLine($"Average time for {num_operations} merge operations with conflicts: {average_time} ms");
        }
    }
}